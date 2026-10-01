using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// F4: GET /clubos/venues/{venueId}/payouts[/{id}] — read-only payouts for venue owners, scoped to the partner running the venue.
/// Every test builds its own partners / venues / payouts / bookings in SQL and removes them afterwards; the shared seed is untouched.
/// </summary>
[Collection(ApiCollection.Name)]
public class ClubOsPayoutsTests(ApiFactory api) : IAsyncLifetime
{
    private readonly List<Guid> _partners = [];
    private readonly List<Guid> _users = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => api.WithDb(async db =>
    {
        foreach (var p in _partners)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET payout_id = NULL WHERE payout_id IN (SELECT id FROM payouts WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM bookings WHERE venue_id IN (SELECT id FROM venues WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venue_members WHERE venue_id IN (SELECT id FROM venues WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
        }
    });

    private sealed record PV(Guid PartnerId, Guid VenueId, Guid CourtId, string NameEn);

    private static string Url(Guid venue, string tail = "") => $"/api/v1/clubos/venues/{venue}/payouts{tail}";

    private async Task<Guid> NewPartnerAsync(string? bank = "KBank", string? account = "123-4-56789-0")
    {
        var id = Guid.NewGuid();
        var legal = "F4 Partner " + id.ToString("N")[..8];
        _partners.Add(id);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO partners (id, legal_name, bank_name, bank_account, status) VALUES ({id}, {legal}, {bank}, {account}, 'active')
            """));
        return id;
    }

    private async Task<PV> NewVenueAsync(Guid partner)
    {
        var v = Guid.NewGuid();
        var c = Guid.NewGuid();
        var slug = "f4-" + v.ToString("N")[..12];
        var name = "F4 Venue " + slug;
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({v}, {partner}, {slug}, 'ทดสอบ', {name}, 'Test', 'x', 13.7, 100.5, 'hidden')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO courts (id, venue_id, name, surface, sort_order) VALUES ({c}, {v}, 'A', 'hard', 1)");
        });
        return new PV(partner, v, c, name);
    }

    private async Task<PV> NewPartnerVenueAsync() => await NewVenueAsync(await NewPartnerAsync());

    private async Task<Guid> AddPayoutAsync(Guid partner, string start, string status = "pending", decimal gross = 1000, decimal commission = 100,
        string? transferRef = null)
    {
        var id = Guid.NewGuid();
        var s = DateOnly.Parse(start);
        var e = s.AddDays(7);
        var net = gross - commission;
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO payouts (id, partner_id, period_start, period_end, gross, commission, net, status, transfer_ref)
            VALUES ({id}, {partner}, {s}, {e}, {gross}, {commission}, {net}, {status}, {transferRef})
            """));
        return id;
    }

    /// <summary>Completed booking settled in <paramref name="payout"/> with one slot starting at startUtc.</summary>
    private async Task<Guid> AddSettledAsync(PV v, Guid payout, DateTime startUtc, decimal total, decimal commission)
    {
        var id = Guid.NewGuid();
        var end = startUtc.AddHours(1);
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO bookings (id, venue_id, status, source, subtotal, addon_total, total, commission, payout_id)
                VALUES ({id}, {v.VenueId}, 'completed', 'app', {total}, 0, {total}, {commission}, {payout})
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO booking_slots (booking_id, court_id, start_at, end_at, price, status)
                VALUES ({id}, {v.CourtId}, {startUtc}, {end}, {total}, 'completed')
                """);
        });
        return id;
    }

    private async Task<TestUser> OwnerOfAsync(Guid venue) => await api.LoginPartnerAsync(venue, "owner", "partner_owner");

    private static async Task<JsonElement> JsonOf(HttpResponseMessage res, HttpStatusCode expected)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"expected {(int)expected} but got {(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement;
    }

    private static Guid[] Ids(JsonElement page) => page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToArray();

    // ------------------------------------------------------------------ authorization

    [Fact]
    public async Task Anonymous_gets_401_json_on_list_and_detail()
    {
        var pv = await NewPartnerVenueAsync();
        var pid = await AddPayoutAsync(pv.PartnerId, "2026-01-02");
        foreach (var url in new[] { Url(pv.VenueId), Url(pv.VenueId, "/" + pid) })
        {
            var res = await api.Anonymous().GetAsync(url);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
            Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task Player_gets_403()
    {
        var pv = await NewPartnerVenueAsync();
        var pid = await AddPayoutAsync(pv.PartnerId, "2026-01-02");
        var player = await api.LoginAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await player.Http.GetAsync(Url(pv.VenueId))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await player.Http.GetAsync(Url(pv.VenueId, "/" + pid))).StatusCode);
    }

    [Fact]
    public async Task Staff_of_the_venue_gets_403()
    {
        var pv = await NewPartnerVenueAsync();
        var pid = await AddPayoutAsync(pv.PartnerId, "2026-01-02");
        var staff = await api.LoginPartnerAsync(pv.VenueId, "staff");
        var res = await staff.Http.GetAsync(Url(pv.VenueId));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.Http.GetAsync(Url(pv.VenueId, "/" + pid))).StatusCode);
    }

    [Fact]
    public async Task Owner_of_another_venue_gets_403()
    {
        var a = await NewPartnerVenueAsync();
        var b = await NewPartnerVenueAsync();
        var pid = await AddPayoutAsync(b.PartnerId, "2026-01-02");
        var ownerA = await OwnerOfAsync(a.VenueId);
        Assert.Equal(HttpStatusCode.Forbidden, (await ownerA.Http.GetAsync(Url(b.VenueId))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await ownerA.Http.GetAsync(Url(b.VenueId, "/" + pid))).StatusCode);
    }

    [Fact]
    public async Task Partner_user_without_membership_gets_403()
    {
        var pv = await NewPartnerVenueAsync();
        var nobody = await api.LoginPartnerAsync(venueId: null, "owner", "partner_owner");
        Assert.Equal(HttpStatusCode.Forbidden, (await nobody.Http.GetAsync(Url(pv.VenueId))).StatusCode);
    }

    [Fact]
    public async Task Owner_gets_200_and_empty_page_when_partner_has_no_payouts()
    {
        var pv = await NewPartnerVenueAsync();
        var owner = await OwnerOfAsync(pv.VenueId);
        var page = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId)), HttpStatusCode.OK);
        Assert.Equal(0, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(25, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(0, page.GetProperty("items").GetArrayLength());
    }

    // ------------------------------------------------------------------ partner scoping

    [Fact]
    public async Task List_never_contains_another_partners_payouts()
    {
        var a = await NewPartnerVenueAsync();
        var b = await NewPartnerVenueAsync();
        var pa = await AddPayoutAsync(a.PartnerId, "2026-01-02");
        var pb1 = await AddPayoutAsync(b.PartnerId, "2026-01-02");
        var pb2 = await AddPayoutAsync(b.PartnerId, "2026-01-09");
        var ownerA = await OwnerOfAsync(a.VenueId);
        var ownerB = await OwnerOfAsync(b.VenueId);

        var la = await JsonOf(await ownerA.Http.GetAsync(Url(a.VenueId)), HttpStatusCode.OK);
        Assert.Equal(1, la.GetProperty("total").GetInt32());
        Assert.Equal(new[] { pa },Ids(la));
        Assert.All(la.GetProperty("items").EnumerateArray(), i => Assert.Equal(a.PartnerId, i.GetProperty("partnerId").GetGuid()));

        var lb = await JsonOf(await ownerB.Http.GetAsync(Url(b.VenueId)), HttpStatusCode.OK);
        Assert.Equal(2, lb.GetProperty("total").GetInt32());
        Assert.Equal(new[] { pb2, pb1 },Ids(lb));
    }

    [Fact]
    public async Task Detail_of_another_partners_payout_is_404_and_leaks_nothing()
    {
        var a = await NewPartnerVenueAsync();
        var b = await NewPartnerVenueAsync();
        var pb = await AddPayoutAsync(b.PartnerId, "2026-01-02", transferRef: "SECRET-REF");
        await AddSettledAsync(b, pb, new DateTime(2026, 1, 3, 3, 0, 0, DateTimeKind.Utc), 500, 50);
        var ownerA = await OwnerOfAsync(a.VenueId);

        var res = await ownerA.Http.GetAsync(Url(a.VenueId, "/" + pb));
        var raw = await res.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.DoesNotContain("SECRET-REF", raw);
        Assert.DoesNotContain(b.NameEn, raw);
        Assert.DoesNotContain(b.PartnerId.ToString(), raw);
    }

    [Fact]
    public async Task Multi_venue_partner_payout_includes_bookings_of_both_venues_and_either_owner_sees_it()
    {
        var partner = await NewPartnerAsync();
        var v1 = await NewVenueAsync(partner);
        var v2 = await NewVenueAsync(partner);
        var pid = await AddPayoutAsync(partner, "2026-01-02", gross: 300, commission: 30);
        var b1 = await AddSettledAsync(v1, pid, new DateTime(2026, 1, 3, 3, 0, 0, DateTimeKind.Utc), 100, 10);
        var b2 = await AddSettledAsync(v2, pid, new DateTime(2026, 1, 4, 3, 0, 0, DateTimeKind.Utc), 200, 20);

        foreach (var pv in new[] { v1, v2 })
        {
            var owner = await OwnerOfAsync(pv.VenueId);
            var list = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId)), HttpStatusCode.OK);
            Assert.Equal(new[] { pid }, Ids(list));
            Assert.Equal(2, list.GetProperty("items")[0].GetProperty("bookingCount").GetInt32());

            var d = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId, "/" + pid)), HttpStatusCode.OK);
            var bookings = d.GetProperty("bookings").EnumerateArray().ToList();
            Assert.Equal(new[] { b1, b2 }.Order(), bookings.Select(x => x.GetProperty("id").GetGuid()).Order());
            Assert.Equal(new[] { v1.NameEn, v2.NameEn }.Order(), bookings.Select(x => x.GetProperty("venueName").GetString()!).Order());
        }
    }

    [Fact]
    public async Task Owner_of_a_partners_second_venue_only_is_still_scoped_to_that_partner()
    {
        // a user who owns venue X of partner A must not read partner B's payouts by passing X and B's payout id
        var a = await NewPartnerVenueAsync();
        var b = await NewPartnerVenueAsync();
        var pb = await AddPayoutAsync(b.PartnerId, "2026-01-02");
        var owner = await OwnerOfAsync(a.VenueId);
        Assert.Equal(HttpStatusCode.NotFound, (await owner.Http.GetAsync(Url(a.VenueId, "/" + pb))).StatusCode);
    }

    // ------------------------------------------------------------------ paging / ordering / clamping

    [Fact]
    public async Task List_is_newest_period_first_and_pages_consistently()
    {
        var pv = await NewPartnerVenueAsync();
        var ids = new List<Guid>();
        // insert out of order, periods 2026-01-02 .. 2026-01-30 (5 weeks)
        foreach (var d in new[] { "2026-01-16", "2026-01-02", "2026-01-30", "2026-01-09", "2026-01-23" })
            ids.Add(await AddPayoutAsync(pv.PartnerId, d));
        var expected = new[] { ids[2], ids[4], ids[0], ids[3], ids[1] };
        var owner = await OwnerOfAsync(pv.VenueId);

        var all = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId)), HttpStatusCode.OK);
        Assert.Equal(expected, Ids(all));

        var p1 = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId) + "?page=1&pageSize=2"), HttpStatusCode.OK);
        var p2 = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId) + "?page=2&pageSize=2"), HttpStatusCode.OK);
        var p3 = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId) + "?page=3&pageSize=2"), HttpStatusCode.OK);
        var p4 = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId) + "?page=4&pageSize=2"), HttpStatusCode.OK);
        Assert.All(new[] { p1, p2, p3, p4 }, p => Assert.Equal(5, p.GetProperty("total").GetInt32()));
        Assert.Equal(expected[..2], Ids(p1));
        Assert.Equal(expected[2..4], Ids(p2));
        Assert.Equal(expected[4..], Ids(p3));
        Assert.Empty(Ids(p4));
        Assert.Equal(2, p2.GetProperty("page").GetInt32());
        Assert.Equal(2, p2.GetProperty("pageSize").GetInt32());
    }

    [Theory]
    [InlineData("pageSize=0", 1, 1)]
    [InlineData("pageSize=-5", 1, 1)]
    [InlineData("pageSize=1000", 1, 100)]
    [InlineData("page=0&pageSize=10", 1, 10)]
    [InlineData("page=-3", 1, 25)]
    public async Task Paging_parameters_are_clamped(string query, int expectedPage, int expectedSize)
    {
        var pv = await NewPartnerVenueAsync();
        await AddPayoutAsync(pv.PartnerId, "2026-01-02");
        await AddPayoutAsync(pv.PartnerId, "2026-01-09");
        var owner = await OwnerOfAsync(pv.VenueId);
        var page = await JsonOf(await owner.Http.GetAsync(Url(pv.VenueId) + "?" + query), HttpStatusCode.OK);
        Assert.Equal(expectedPage, page.GetProperty("page").GetInt32());
        Assert.Equal(expectedSize, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(Math.Min(2, expectedSize), page.GetProperty("items").GetArrayLength());
    }

    // ------------------------------------------------------------------ detail

    [Fact]
    public async Task Detail_lists_exactly_the_settled_bookings_with_totals_and_local_date()
    {
        var partner = await NewPartnerAsync("SCB", "999-9-99999-9");
        var v = await NewVenueAsync(partner);
        var pid = await AddPayoutAsync(partner, "2026-01-02", "paid", gross: 700, commission: 70, transferRef: "KBANK-1");
        var other = await AddPayoutAsync(partner, "2026-01-09", gross: 50, commission: 5);
        // 2026-01-03 18:30 UTC = 2026-01-04 01:30 Bangkok -> local date must be the 4th
        var b1 = await AddSettledAsync(v, pid, new DateTime(2026, 1, 3, 18, 30, 0, DateTimeKind.Utc), 400, 40);
        var b2 = await AddSettledAsync(v, pid, new DateTime(2026, 1, 5, 3, 0, 0, DateTimeKind.Utc), 300, 30);
        var notMine = await AddSettledAsync(v, other, new DateTime(2026, 1, 10, 3, 0, 0, DateTimeKind.Utc), 50, 5);
        var owner = await OwnerOfAsync(v.VenueId);

        var d = await JsonOf(await owner.Http.GetAsync(Url(v.VenueId, "/" + pid)), HttpStatusCode.OK);
        var p = d.GetProperty("payout");
        Assert.Equal(pid, p.GetProperty("id").GetGuid());
        Assert.Equal(partner, p.GetProperty("partnerId").GetGuid());
        Assert.Equal("2026-01-02", p.GetProperty("periodStart").GetString());
        Assert.Equal("2026-01-09", p.GetProperty("periodEnd").GetString());
        Assert.Equal(700m, p.GetProperty("gross").GetDecimal());
        Assert.Equal(70m, p.GetProperty("commission").GetDecimal());
        Assert.Equal(630m, p.GetProperty("net").GetDecimal());
        Assert.Equal("paid", p.GetProperty("status").GetString());
        Assert.Equal("KBANK-1", p.GetProperty("transferRef").GetString());
        Assert.Equal(2, p.GetProperty("bookingCount").GetInt32());
        Assert.Equal("SCB", p.GetProperty("bankName").GetString());
        Assert.Equal("999-9-99999-9", p.GetProperty("bankAccount").GetString());
        Assert.StartsWith("F4 Partner", p.GetProperty("partnerName").GetString());

        var rows = d.GetProperty("bookings").EnumerateArray().ToList();
        Assert.Equal(new[] { b1, b2 }.Order(), rows.Select(x => x.GetProperty("id").GetGuid()).Order());
        Assert.DoesNotContain(rows, x => x.GetProperty("id").GetGuid() == notMine);
        Assert.Equal(700m, rows.Sum(x => x.GetProperty("total").GetDecimal()));
        Assert.Equal(70m, rows.Sum(x => x.GetProperty("commission").GetDecimal()));
        var r1 = rows.Single(x => x.GetProperty("id").GetGuid() == b1);
        Assert.Equal("2026-01-04", r1.GetProperty("date").GetString());
        Assert.Equal(400m, r1.GetProperty("total").GetDecimal());
        Assert.Equal(40m, r1.GetProperty("commission").GetDecimal());
        Assert.Equal(v.NameEn, r1.GetProperty("venueName").GetString());
        Assert.False(string.IsNullOrEmpty(r1.GetProperty("code").GetString()));
    }

    [Fact]
    public async Task List_carries_bank_fields_and_null_bank_when_partner_has_none()
    {
        var withBank = await NewPartnerVenueAsync();
        await AddPayoutAsync(withBank.PartnerId, "2026-01-02");
        var partner = await NewPartnerAsync(bank: null, account: null);
        var noBank = await NewVenueAsync(partner);
        await AddPayoutAsync(partner, "2026-01-02");

        var o1 = await OwnerOfAsync(withBank.VenueId);
        var i1 = (await JsonOf(await o1.Http.GetAsync(Url(withBank.VenueId)), HttpStatusCode.OK)).GetProperty("items")[0];
        Assert.Equal("KBank", i1.GetProperty("bankName").GetString());
        Assert.Equal("123-4-56789-0", i1.GetProperty("bankAccount").GetString());

        var o2 = await OwnerOfAsync(noBank.VenueId);
        var i2 = (await JsonOf(await o2.Http.GetAsync(Url(noBank.VenueId)), HttpStatusCode.OK)).GetProperty("items")[0];
        Assert.Equal(JsonValueKind.Null, i2.GetProperty("bankName").ValueKind);
        Assert.Equal(JsonValueKind.Null, i2.GetProperty("bankAccount").ValueKind);
    }

    [Fact]
    public async Task Unknown_payout_id_is_404_json()
    {
        var pv = await NewPartnerVenueAsync();
        var owner = await OwnerOfAsync(pv.VenueId);
        var res = await owner.Http.GetAsync(Url(pv.VenueId, "/" + Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Non_guid_payout_id_is_404_not_500()
    {
        var pv = await NewPartnerVenueAsync();
        var owner = await OwnerOfAsync(pv.VenueId);
        var res = await owner.Http.GetAsync(Url(pv.VenueId, "/not-a-guid"));
        Assert.True((int)res.StatusCode < 500, $"got {(int)res.StatusCode}");
    }

    [Fact]
    public async Task Unknown_venue_id_is_not_a_server_error_for_owner()
    {
        var pv = await NewPartnerVenueAsync();
        var owner = await OwnerOfAsync(pv.VenueId);
        var res = await owner.Http.GetAsync(Url(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Endpoints_are_read_only()
    {
        var pv = await NewPartnerVenueAsync();
        var pid = await AddPayoutAsync(pv.PartnerId, "2026-01-02");
        var owner = await OwnerOfAsync(pv.VenueId);
        foreach (var req in new[]
        {
            new HttpRequestMessage(HttpMethod.Post, Url(pv.VenueId)),
            new HttpRequestMessage(HttpMethod.Delete, Url(pv.VenueId, "/" + pid)),
            new HttpRequestMessage(HttpMethod.Post, Url(pv.VenueId, "/" + pid + "/mark-paid")),
        })
        {
            var res = await owner.Http.SendAsync(req);
            Assert.True(res.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotFound, $"{req.Method} {req.RequestUri} -> {(int)res.StatusCode}");
        }
        await api.WithDb(async db => Assert.Equal(1, await db.Payouts.CountAsync(p => p.PartnerId == pv.PartnerId)));
    }

    // ------------------------------------------------------------------ admin unchanged

    private async Task<HttpClient> AdminAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync("/api/v1/auth/admin/login", new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    [Fact]
    public async Task Admin_endpoints_still_return_the_same_shapes_and_owner_cannot_use_them()
    {
        var partner = await NewPartnerAsync();
        var v = await NewVenueAsync(partner);
        var pid = await AddPayoutAsync(partner, "2026-01-02", gross: 100, commission: 10);
        var bid = await AddSettledAsync(v, pid, new DateTime(2026, 1, 3, 3, 0, 0, DateTimeKind.Utc), 100, 10);
        var admin = await AdminAsync();

        var list = await JsonOf(await admin.GetAsync($"/api/v1/admin/payouts?partnerId={partner}"), HttpStatusCode.OK);
        Assert.Equal(1, list.GetProperty("total").GetInt32());
        Assert.Equal(new[] { pid }, Ids(list));
        Assert.Equal(1, list.GetProperty("items")[0].GetProperty("bookingCount").GetInt32());

        var d = await JsonOf(await admin.GetAsync($"/api/v1/admin/payouts/{pid}"), HttpStatusCode.OK);
        Assert.Equal(bid, d.GetProperty("bookings")[0].GetProperty("id").GetGuid());
        Assert.Equal(90m, d.GetProperty("payout").GetProperty("net").GetDecimal());

        var owner = await OwnerOfAsync(v.VenueId);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.Http.GetAsync("/api/v1/admin/payouts")).StatusCode);
    }
}
