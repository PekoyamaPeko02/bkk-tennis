using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// A3: GET /admin/bookings (+ .csv). Every test works on its own private, hidden <see cref="ScheduleVenue"/> and filters by its
/// venueId so assertions are exact; the shared DB is cleaned in <see cref="DisposeAsync"/> (bookings, payments, users, venues).
/// </summary>
[Collection(ApiCollection.Name)]
public class AdminBookingExplorerTests(ApiFactory api) : IAsyncLifetime
{
    private const string Json = "/api/v1/admin/bookings", Csv = "/api/v1/admin/bookings.csv";
    private const string AdminLogin = "/api/v1/auth/admin/login";

    private readonly List<ScheduleVenue> _venues = [];
    private readonly List<Guid> _users = [];
    private readonly int _errorsBefore = api.ServerErrors.Count;
    private static int _phoneSeq = Random.Shared.Next(0, 900);

    private DateOnly Today => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
    private DateOnly Future => Today.AddDays(20);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var v in _venues)
        {
            await api.WithDb(async db =>
            {
                var owners = await db.Database.SqlQuery<Guid>($"SELECT user_id AS \"Value\" FROM bookings WHERE venue_id = {v.Id} AND user_id IS NOT NULL").ToListAsync();
                var members = await db.Database.SqlQuery<Guid>($"SELECT user_id AS \"Value\" FROM venue_members WHERE venue_id = {v.Id}").ToListAsync();
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM refunds WHERE payment_id IN (SELECT p.id FROM payments p JOIN bookings b ON b.id = p.booking_id WHERE b.venue_id = {v.Id})");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payments WHERE booking_id IN (SELECT id FROM bookings WHERE venue_id = {v.Id})");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM booking_addons WHERE booking_id IN (SELECT id FROM bookings WHERE venue_id = {v.Id})");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM booking_slots WHERE booking_id IN (SELECT id FROM bookings WHERE venue_id = {v.Id})");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM bookings WHERE venue_id = {v.Id}");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE id = {v.Id}");
                foreach (var u in owners.Concat(members).Distinct())
                    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u} AND role IN ('player','partner_staff')");
            });
        }
        foreach (var u in _users)
            await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u}"));
    }

    // ------------------------------------------------------------------ helpers

    private async Task<ScheduleVenue> NewVenueAsync()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        _venues.Add(v);
        return v;
    }

    private static string Phone(string first3, string last4) => "+66" + first3[1..] + (Interlocked.Increment(ref _phoneSeq) % 1000).ToString("D3") + last4;

    private async Task<HttpClient> AdminAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync(AdminLogin, new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private async Task<JsonElement> GetAsync(HttpClient admin, string query, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var res = await admin.GetAsync(Json + query);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"GET {query}: expected {(int)expected} got {(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement.Clone();
    }

    private static string[] Codes(JsonElement page) => page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("code").GetString()!).ToArray();
    private static JsonElement Item(JsonElement page, string code) => page.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("code").GetString() == code);

    private async Task<string> CodeOfAsync(Guid booking)
    {
        var code = "";
        await api.WithDb(async db => code = (await db.Database.SqlQuery<string>($"SELECT code AS \"Value\" FROM bookings WHERE id = {booking}").ToListAsync()).Single());
        return code;
    }

    private Task SetCreatedAsync(Guid booking, DateTime utc) =>
        api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET created_at = {utc} WHERE id = {booking}"));

    private Task SetCommissionAsync(Guid booking, decimal commission) =>
        api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET commission = {commission} WHERE id = {booking}"));

    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd");

    private async Task<(string Raw, HttpResponseMessage Res)> CsvAsync(HttpClient admin, string query)
    {
        var res = await admin.GetAsync(Csv + query);
        // HttpClient's string reader strips the BOM: decode the bytes ourselves so it stays visible
        return (Encoding.UTF8.GetString(await res.Content.ReadAsByteArrayAsync()), res);
    }

    /// <summary>Minimal RFC-4180 reader (handles quotes, doubled quotes, embedded commas/newlines, CRLF row ends).</summary>
    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else cell.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ',') { row.Add(cell.ToString()); cell.Clear(); }
            else if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') { row.Add(cell.ToString()); cell.Clear(); rows.Add(row); row = []; i++; }
            else cell.Append(c);
        }
        if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString()); rows.Add(row); }
        return rows;
    }

    private static List<List<string>> CsvRows(string raw) => ParseCsv(raw.TrimStart('﻿'));

    // ------------------------------------------------------------------ authorization

    [Fact]
    public async Task Both_routes_require_a_token()
    {
        foreach (var url in new[] { Json, Csv })
        {
            var res = await api.Anonymous().GetAsync(url);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
            Assert.Equal("unauthorized", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Both_routes_are_forbidden_for_players_and_partners()
    {
        var player = await api.LoginAsync();
        var owner = await api.LoginPartnerAsync(Seed.VenueId, "owner", "partner_owner");
        var staff = await api.LoginPartnerAsync(Seed.VenueId, "staff", "partner_staff");
        try
        {
            foreach (var u in new[] { player, owner, staff })
                foreach (var url in new[] { Json, Csv })
                {
                    var res = await u.Http.GetAsync(url);
                    Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
                    Assert.Equal("forbidden", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
                }
        }
        finally
        {
            foreach (var u in new[] { player, owner, staff }) _users.Add(u.Id);
            await api.WithDb(async db =>
            {
                foreach (var u in new[] { owner, staff })
                    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venue_members WHERE user_id = {u.Id}");
            });
        }
    }

    // ------------------------------------------------------------------ shape

    [Fact]
    public async Task A_booking_is_listed_with_venue_time_courts_totals_and_a_masked_phone()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00", "11:00"], userDisplayName: "สมชาย ก.", userPhone: Phone("081", "5678"));
        await SetCommissionAsync(id, 80);

        var page = await GetAsync(admin, $"?venueId={v.Id}");

        Assert.Equal(1, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(25, page.GetProperty("pageSize").GetInt32());
        var b = page.GetProperty("items")[0];
        Assert.Equal(id, b.GetProperty("id").GetGuid());
        Assert.Equal(await CodeOfAsync(id), b.GetProperty("code").GetString());
        Assert.Equal("confirmed", b.GetProperty("status").GetString());
        Assert.Equal("app", b.GetProperty("source").GetString());
        Assert.Equal(v.Id, b.GetProperty("venueId").GetGuid());
        Assert.Equal("Schedule Test", b.GetProperty("venueName").GetString());
        Assert.Equal("สมชาย ก.", b.GetProperty("customerName").GetString());
        Assert.Equal("081-***-5678", b.GetProperty("customerPhone").GetString());
        Assert.Equal("paid", b.GetProperty("paymentState").GetString());
        Assert.Equal(D(Future), b.GetProperty("date").GetString());
        Assert.Equal("10:00", b.GetProperty("start").GetString());
        Assert.Equal("12:00", b.GetProperty("end").GetString());
        Assert.Equal(new[] { "คอร์ท A" }, b.GetProperty("courts").EnumerateArray().Select(c => c.GetString()).ToArray());
        Assert.Equal(800m, b.GetProperty("total").GetDecimal());
        Assert.Equal(80m, b.GetProperty("commission").GetDecimal());
        Assert.True(b.TryGetProperty("createdAt", out _));
    }

    [Fact]
    public async Task The_full_phone_number_is_never_returned_and_a_foreign_number_is_masked_too()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var th = Phone("089", "4321");
        var thId = await v.AddBookingAsync(v.CourtA, Future, ["06:00"], userDisplayName: "Thai", userPhone: th);
        var foreign = await v.AddBookingAsync(v.CourtA, Future, ["07:00"], userDisplayName: "Foreign", userPhone: "+1415" + Random.Shared.Next(100, 999) + "0123");

        var res = await admin.GetAsync($"{Json}?venueId={v.Id}");
        var raw = await res.Content.ReadAsStringAsync();
        var page = JsonDocument.Parse(raw).RootElement;

        Assert.DoesNotContain(th, raw);
        Assert.DoesNotContain(th[3..], raw);
        Assert.DoesNotContain("+1415", raw);
        Assert.Equal("089-***-4321", Item(page, await CodeOfAsync(thId)).GetProperty("customerPhone").GetString());
        Assert.Equal("***0123", Item(page, await CodeOfAsync(foreign)).GetProperty("customerPhone").GetString());
    }

    [Fact]
    public async Task A_walk_in_shows_the_typed_name_no_phone_and_counter_payment_even_with_a_payment_row()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], source: "walk_in", customerName: "คุณวิชัย", payment: "succeeded");
        var noPay = await v.AddBookingAsync(v.CourtA, Future, ["11:00"], source: "walk_in", customerName: "No Pay", payment: null);

        var page = await GetAsync(admin, $"?venueId={v.Id}");

        foreach (var (bid, name) in new[] { (id, "คุณวิชัย"), (noPay, "No Pay") })
        {
            var b = Item(page, await CodeOfAsync(bid));
            Assert.Equal("walk_in", b.GetProperty("source").GetString());
            Assert.Equal(name, b.GetProperty("customerName").GetString());
            Assert.Equal(JsonValueKind.Null, b.GetProperty("customerPhone").ValueKind);
            Assert.Equal("counter", b.GetProperty("paymentState").GetString());
        }
    }

    [Fact]
    public async Task Payment_state_follows_the_latest_payment_and_is_none_without_one()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var cases = new Dictionary<string, Guid>();
        var hour = 6;
        foreach (var (state, status) in new[] { ("paid", "succeeded"), ("pending", "pending"), ("failed", "failed"), ("refunded", "refunded") })
            cases[state] = await v.AddBookingAsync(v.CourtA, Future, [$"{hour++:00}:00"], payment: status);
        cases["none"] = await v.AddBookingAsync(v.CourtA, Future, [$"{hour++:00}:00"], payment: null);
        // older pending, newer succeeded => paid ; older succeeded, newer failed => failed
        var newerWins = await v.AddBookingAsync(v.CourtA, Future, [$"{hour++:00}:00"], payment: "pending");
        await v.AddPaymentAsync(newerWins, "succeeded", DateTime.UtcNow.AddMinutes(5));
        var laterFails = await v.AddBookingAsync(v.CourtA, Future, [$"{hour++:00}:00"], payment: "succeeded");
        await v.AddPaymentAsync(laterFails, "failed", DateTime.UtcNow.AddMinutes(5));

        var page = await GetAsync(admin, $"?venueId={v.Id}");

        foreach (var (state, id) in cases)
            Assert.Equal(state, Item(page, await CodeOfAsync(id)).GetProperty("paymentState").GetString());
        Assert.Equal("paid", Item(page, await CodeOfAsync(newerWins)).GetProperty("paymentState").GetString());
        Assert.Equal("failed", Item(page, await CodeOfAsync(laterFails)).GetProperty("paymentState").GetString());
    }

    [Fact]
    public async Task A_multi_slot_booking_spans_first_start_to_last_end_and_lists_each_court_once()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var id = await v.AddBookingAsync(v.CourtA, Future, ["14:00", "08:00", "09:00"]); // inserted out of order
        await v.AddSlotAsync(id, v.CourtB, Future, "10:00");
        var code = await CodeOfAsync(id);

        var b = Item(await GetAsync(admin, $"?venueId={v.Id}"), code);

        Assert.Equal("08:00", b.GetProperty("start").GetString());
        Assert.Equal("15:00", b.GetProperty("end").GetString());
        Assert.Equal(D(Future), b.GetProperty("date").GetString());
        Assert.Equal(new[] { "คอร์ท A", "คอร์ท B" }, b.GetProperty("courts").EnumerateArray().Select(c => c.GetString()).ToArray());
        Assert.Equal(1, (await GetAsync(admin, $"?venueId={v.Id}")).GetProperty("total").GetInt32()); // joined rows never duplicate the booking
    }

    [Fact]
    public async Task A_booking_without_slots_lists_with_null_time_and_no_courts()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var id = Guid.NewGuid();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bookings (id, venue_id, status, source, customer_name, subtotal, total) VALUES ({id}, {v.Id}, 'expired', 'walk_in', 'Empty', 0, 0)
            """));

        var page = await GetAsync(admin, $"?venueId={v.Id}");

        var b = page.GetProperty("items")[0];
        Assert.Equal(JsonValueKind.Null, b.GetProperty("date").ValueKind);
        Assert.Equal(JsonValueKind.Null, b.GetProperty("start").ValueKind);
        Assert.Equal(JsonValueKind.Null, b.GetProperty("end").ValueKind);
        Assert.Empty(b.GetProperty("courts").EnumerateArray());
    }

    // ------------------------------------------------------------------ filters

    [Fact]
    public async Task Venue_filter_only_returns_that_venue_and_an_unknown_venue_is_empty()
    {
        var v1 = await NewVenueAsync();
        var v2 = await NewVenueAsync();
        var admin = await AdminAsync();
        var a = await v1.AddBookingAsync(v1.CourtA, Future, ["10:00"]);
        var b = await v2.AddBookingAsync(v2.CourtA, Future, ["10:00"]);

        var p1 = await GetAsync(admin, $"?venueId={v1.Id}");
        var p2 = await GetAsync(admin, $"?venueId={v2.Id}");
        var none = await GetAsync(admin, $"?venueId={Guid.NewGuid()}");

        Assert.Equal(new[] { await CodeOfAsync(a) }, Codes(p1));
        Assert.Equal(new[] { await CodeOfAsync(b) }, Codes(p2));
        Assert.Equal(0, none.GetProperty("total").GetInt32());
        Assert.Empty(none.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task Status_filter_returns_only_that_status_for_each_of_the_seven_statuses()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var statuses = new[] { "held", "confirmed", "checked_in", "completed", "expired", "cancelled", "no_show" };
        var byStatus = new Dictionary<string, string>();
        for (var i = 0; i < statuses.Length; i++)
            byStatus[statuses[i]] = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, [$"{8 + i:00}:00"], status: statuses[i],
                holdExpiresAt: statuses[i] == "held" ? DateTime.UtcNow.AddDays(30) : null));

        foreach (var s in statuses)
        {
            var page = await GetAsync(admin, $"?venueId={v.Id}&status={s}");
            Assert.Equal(new[] { byStatus[s] }, Codes(page));
            Assert.Equal(1, page.GetProperty("total").GetInt32());
        }
        Assert.Equal(7, (await GetAsync(admin, $"?venueId={v.Id}")).GetProperty("total").GetInt32());
    }

    [Theory]
    [InlineData("status=paid", "bad_status")]
    [InlineData("status=Confirmed", "bad_status")]
    [InlineData("source=web", "bad_source")]
    [InlineData("source=Walk_In", "bad_source")]
    [InlineData("from=2026-05-10&to=2026-05-09", "bad_range")]
    public async Task Bad_filters_are_400_with_a_code_on_both_routes(string query, string code)
    {
        var admin = await AdminAsync();

        foreach (var url in new[] { Json, Csv })
        {
            var res = await admin.GetAsync($"{url}?{query}");
            var raw = await res.Content.ReadAsStringAsync();
            Assert.True(res.StatusCode == HttpStatusCode.BadRequest, $"{url}?{query}: {(int)res.StatusCode} {raw}");
            Assert.Equal(code, JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
            Assert.Contains("json", res.Content.Headers.ContentType?.MediaType);
        }
    }

    [Theory]
    [InlineData("from=2026-13-45")]
    [InlineData("from=not-a-date")]
    [InlineData("to=31/12/2026")]
    public async Task An_unparseable_date_is_a_client_error_not_a_500(string query)
    {
        var admin = await AdminAsync();

        var res = await admin.GetAsync($"{Json}?{query}");

        Assert.True((int)res.StatusCode is >= 400 and < 500, $"got {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        // NOTE: the framework logs the binding failure as an unhandled exception (ServerErrors); that is reported, not asserted.
    }

    [Fact]
    public async Task Source_filter_splits_app_and_walk_in()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var app = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userDisplayName: "App User", userPhone: Phone("081", "1111")));
        var walk = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["11:00"], source: "walk_in", customerName: "Walker"));

        Assert.Equal(new[] { app }, Codes(await GetAsync(admin, $"?venueId={v.Id}&source=app")));
        Assert.Equal(new[] { walk }, Codes(await GetAsync(admin, $"?venueId={v.Id}&source=walk_in")));
    }

    [Fact]
    public async Task Date_range_is_inclusive_on_the_bangkok_play_date_with_boundaries_at_17_00_utc()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var d = Future;
        // Bangkok 00:00 on d is 17:00 UTC on d-1; Bangkok 23:59 on d is 16:59 UTC on d.
        var before = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d.AddDays(-1), ["23:00"]));   // 16:00 UTC d-1: still d-1
        var midnight = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d, ["00:00"]));            // exactly 17:00 UTC d-1: d
        var noon = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d, ["12:00"]));
        var late = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d, ["23:00"]));                // 16:00 UTC d
        var next = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d.AddDays(1), ["00:00"]));     // 17:00 UTC d: d+1
        Assert.Equal(new DateTime(d.Year, d.Month, d.Day, 17, 0, 0, DateTimeKind.Utc).AddDays(-1), VenueClock.ToUtc(d, TimeOnly.MinValue, Seed.Timezone)); // sanity of the premise

        async Task<string[]> Q(string range) => Codes(await GetAsync(admin, $"?venueId={v.Id}&{range}")).OrderBy(x => x).ToArray();
        string[] Sorted(params string[] c) => c.OrderBy(x => x).ToArray();

        Assert.Equal(Sorted(midnight, noon, late), await Q($"from={D(d)}&to={D(d)}"));                   // from == to
        Assert.Equal(Sorted(midnight, noon, late, next), await Q($"from={D(d)}"));
        Assert.Equal(Sorted(before, midnight, noon, late), await Q($"to={D(d)}"));
        Assert.Equal(Sorted(before), await Q($"from={D(d.AddDays(-1))}&to={D(d.AddDays(-1))}"));
        Assert.Equal(Sorted(next), await Q($"from={D(d.AddDays(1))}&to={D(d.AddDays(1))}"));
        Assert.Equal(Sorted(before, midnight, noon, late, next), await Q($"from={D(d.AddDays(-1))}&to={D(d.AddDays(1))}"));
        Assert.Empty(await Q($"from={D(d.AddDays(2))}"));
        Assert.Empty(await Q($"to={D(d.AddDays(-2))}"));
    }

    [Fact]
    public async Task Play_date_is_the_first_slot_even_when_a_later_slot_falls_on_another_day()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var d = Future;
        var id = await v.AddBookingAsync(v.CourtA, d, ["22:00"]);
        await v.AddSlotAsync(id, v.CourtA, d.AddDays(1), "09:00");
        var code = await CodeOfAsync(id);

        Assert.Equal(new[] { code }, Codes(await GetAsync(admin, $"?venueId={v.Id}&from={D(d)}&to={D(d)}")));
        Assert.Empty(Codes(await GetAsync(admin, $"?venueId={v.Id}&from={D(d.AddDays(1))}&to={D(d.AddDays(1))}")));
        Assert.Equal(new[] { code }, Codes(await GetAsync(admin, $"?venueId={v.Id}&from={D(d)}")));
    }

    [Fact]
    public async Task Bookings_without_slots_never_match_a_date_filter_but_do_appear_without_one()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var empty = Guid.NewGuid();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bookings (id, venue_id, status, source, customer_name, subtotal, total) VALUES ({empty}, {v.Id}, 'expired', 'walk_in', 'Empty', 0, 0)
            """));
        var real = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["10:00"]));

        Assert.Equal(2, (await GetAsync(admin, $"?venueId={v.Id}")).GetProperty("total").GetInt32());
        Assert.Equal(new[] { real }, Codes(await GetAsync(admin, $"?venueId={v.Id}&from=2000-01-01")));
        Assert.Equal(new[] { real }, Codes(await GetAsync(admin, $"?venueId={v.Id}&to=2100-01-01")));
        Assert.Equal(new[] { real }, Codes(await GetAsync(admin, $"?venueId={v.Id}&from=2000-01-01&to=2100-01-01")));
    }

    [Fact]
    public async Task Text_search_matches_code_walk_in_name_and_player_name_case_insensitively_and_ignores_padding()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var walk = await v.AddBookingAsync(v.CourtA, Future, ["06:00"], source: "walk_in", customerName: "Walk-in Charlie");
        var player = await v.AddBookingAsync(v.CourtA, Future, ["07:00"], userDisplayName: "Dana Player", userPhone: Phone("081", "2222"));
        var thai = await v.AddBookingAsync(v.CourtA, Future, ["08:00"], userDisplayName: "สมหญิง ใจดี", userPhone: Phone("081", "3333"));
        string walkCode = await CodeOfAsync(walk), playerCode = await CodeOfAsync(player), thaiCode = await CodeOfAsync(thai);

        async Task<string[]> Q(string q) => Codes(await GetAsync(admin, $"?venueId={v.Id}&q={Uri.EscapeDataString(q)}")).OrderBy(x => x).ToArray();

        Assert.Equal(new[] { walkCode }, await Q("charlie"));
        Assert.Equal(new[] { walkCode }, await Q("CHARLIE"));
        Assert.Equal(new[] { walkCode }, await Q("  Walk-in  "));
        Assert.Equal(new[] { playerCode }, await Q("dana pl"));
        Assert.Equal(new[] { thaiCode }, await Q("ใจดี"));
        Assert.Equal(new[] { thaiCode }, await Q("สมหญิง"));
        Assert.Equal(new[] { walkCode }, await Q(walkCode.ToLowerInvariant()));
        Assert.Equal(new[] { playerCode }, await Q(playerCode[1..])); // partial code
        Assert.Empty(await Q("nobody-by-this-name"));
        Assert.Equal(3, (await Q("   ")).Length); // blank search = no filter
        Assert.Equal(3, Codes(await GetAsync(admin, $"?venueId={v.Id}&q=")).Length);
    }

    [Fact]
    public async Task Text_search_treats_percent_underscore_and_backslash_literally()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var pct = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["06:00"], source: "walk_in", customerName: "100% Tennis"));
        var under = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["07:00"], source: "walk_in", customerName: "a_b"));
        var plain = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["08:00"], source: "walk_in", customerName: "axb"));
        var slash = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, Future, ["09:00"], source: "walk_in", customerName: @"back\slash"));

        async Task<string[]> Q(string q) => Codes(await GetAsync(admin, $"?venueId={v.Id}&q={Uri.EscapeDataString(q)}")).OrderBy(x => x).ToArray();

        async Task Expect(string q, params string[] codes) =>
            Assert.True(codes.OrderBy(x => x).SequenceEqual(await Q(q)), $"q=[{q}] expected [{string.Join(",", codes)}] got [{string.Join(",", await Q(q))}] (pct={pct} under={under} plain={plain} slash={slash})");

        await Expect("%", pct);
        await Expect("0% t", pct);
        await Expect("_", under);     // would match everything if _ were a wildcard
        await Expect("a_b", under);   // "axb" must not match
        await Expect(@"\", slash);
        await Expect(@"k\s", slash);
        await Expect("%%");
        await Expect(@"\%");
    }

    [Fact]
    public async Task All_filters_combine_with_and()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var d = Future;
        var hit = await CodeOfAsync(await v.AddBookingAsync(v.CourtA, d, ["10:00"], status: "completed", source: "walk_in", customerName: "Target Person"));
        await v.AddBookingAsync(v.CourtA, d, ["11:00"], status: "confirmed", source: "walk_in", customerName: "Target Person");           // wrong status
        await v.AddBookingAsync(v.CourtA, d, ["12:00"], status: "completed", source: "app", userDisplayName: "Target Person", userPhone: Phone("081", "4444")); // wrong source
        await v.AddBookingAsync(v.CourtA, d.AddDays(3), ["10:00"], status: "completed", source: "walk_in", customerName: "Target Person"); // wrong date
        await v.AddBookingAsync(v.CourtA, d, ["13:00"], status: "completed", source: "walk_in", customerName: "Someone Else");         // wrong text
        var other = await NewVenueAsync();
        await other.AddBookingAsync(other.CourtA, d, ["10:00"], status: "completed", source: "walk_in", customerName: "Target Person"); // wrong venue

        var page = await GetAsync(admin, $"?venueId={v.Id}&status=completed&source=walk_in&from={D(d)}&to={D(d.AddDays(1))}&q=target");

        Assert.Equal(new[] { hit }, Codes(page));
        Assert.Equal(1, page.GetProperty("total").GetInt32());
    }

    // ------------------------------------------------------------------ order + paging

    [Fact]
    public async Task Newest_created_first_with_id_as_the_tiebreaker()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var t = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var oldest = await v.AddBookingAsync(v.CourtA, Future, ["06:00"]);
        var newest = await v.AddBookingAsync(v.CourtA, Future, ["07:00"]);
        var mid = await v.AddBookingAsync(v.CourtA, Future, ["08:00"]);
        await SetCreatedAsync(oldest, t);
        await SetCreatedAsync(mid, t.AddDays(1));
        await SetCreatedAsync(newest, t.AddDays(2));
        var tieA = await v.AddBookingAsync(v.CourtA, Future, ["09:00"]);
        var tieB = await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);
        var tieC = await v.AddBookingAsync(v.CourtA, Future, ["11:00"]);
        foreach (var x in new[] { tieA, tieB, tieC }) await SetCreatedAsync(x, t.AddDays(-5));

        var page = await GetAsync(admin, $"?venueId={v.Id}");

        var ids = page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToList();
        Assert.Equal(new[] { newest, mid, oldest }, ids.Take(3).ToArray());
        Assert.Equal(new[] { tieA, tieB, tieC }.Select(g => g.ToString()).OrderBy(s => s, StringComparer.Ordinal).ToArray(), ids.Skip(3).Select(g => g.ToString()).ToArray());
    }

    [Fact]
    public async Task Paging_reports_the_full_total_and_pages_never_overlap()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var t = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var expected = new List<string>();
        for (var i = 0; i < 7; i++)
        {
            var id = await v.AddBookingAsync(v.CourtA, Future.AddDays(i), ["10:00"]);
            await SetCreatedAsync(id, t.AddHours(i));
            expected.Insert(0, await CodeOfAsync(id)); // newest first
        }

        var p1 = await GetAsync(admin, $"?venueId={v.Id}&page=1&pageSize=3");
        var p2 = await GetAsync(admin, $"?venueId={v.Id}&page=2&pageSize=3");
        var p3 = await GetAsync(admin, $"?venueId={v.Id}&page=3&pageSize=3");
        var p4 = await GetAsync(admin, $"?venueId={v.Id}&page=4&pageSize=3");
        var far = await GetAsync(admin, $"?venueId={v.Id}&page=999&pageSize=3");

        foreach (var p in new[] { p1, p2, p3, p4, far }) Assert.Equal(7, p.GetProperty("total").GetInt32());
        Assert.Equal(expected.Take(3), Codes(p1));
        Assert.Equal(expected.Skip(3).Take(3), Codes(p2));
        Assert.Equal(expected.Skip(6), Codes(p3));
        Assert.Empty(p4.GetProperty("items").EnumerateArray());
        Assert.Empty(far.GetProperty("items").EnumerateArray());
        Assert.Equal(4, p4.GetProperty("page").GetInt32());
        Assert.Equal(3, p4.GetProperty("pageSize").GetInt32());
    }

    [Theory]
    [InlineData("pageSize=0", 1, 1)]
    [InlineData("pageSize=-5", 1, 1)]
    [InlineData("pageSize=101", 1, 100)]
    [InlineData("pageSize=100000", 1, 100)]
    [InlineData("page=0", 1, 25)]
    [InlineData("page=-3", 1, 25)]
    [InlineData("", 1, 25)]
    public async Task Page_and_pageSize_are_clamped(string query, int page, int pageSize)
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        for (var i = 0; i < 2; i++) await v.AddBookingAsync(v.CourtA, Future.AddDays(i), ["10:00"]);

        var res = await GetAsync(admin, $"?venueId={v.Id}&{query}");

        Assert.Equal(page, res.GetProperty("page").GetInt32());
        Assert.Equal(pageSize, res.GetProperty("pageSize").GetInt32());
        Assert.Equal(Math.Min(2, pageSize), res.GetProperty("items").GetArrayLength());
        Assert.Equal(2, res.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task The_number_of_sql_commands_does_not_grow_with_the_page_size()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        for (var i = 0; i < 60; i++)
            await v.AddBookingAsync(v.CourtA, Future.AddDays(i), ["10:00"], userDisplayName: $"P{i}", userPhone: Phone("082", i.ToString("D4")),
                addonTotal: 0, payment: i % 3 == 0 ? "pending" : "succeeded");
        await GetAsync(admin, $"?venueId={v.Id}&pageSize=2"); // warm-up: first request compiles queries

        var before = api.SqlCommands;
        var small = await GetAsync(admin, $"?venueId={v.Id}&pageSize=2");
        var smallCount = api.SqlCommands - before;
        before = api.SqlCommands;
        var big = await GetAsync(admin, $"?venueId={v.Id}&pageSize=60");
        var bigCount = api.SqlCommands - before;

        Assert.Equal(2, small.GetProperty("items").GetArrayLength());
        Assert.Equal(60, big.GetProperty("items").GetArrayLength());
        Assert.True(smallCount > 0, "the SQL command counter is not counting");
        Assert.Equal(smallCount, bigCount);
    }

    // ------------------------------------------------------------------ csv

    [Fact]
    public async Task Csv_has_a_bom_crlf_the_documented_header_and_invariant_formatted_cells()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00", "11:00"], userDisplayName: "Somchai", userPhone: Phone("081", "5678"));
        await v.AddSlotAsync(id, v.CourtB, Future, "12:00");
        await SetCommissionAsync(id, 80);
        // 2026-03-01 17:30 UTC = 2026-03-02 00:30 in Bangkok
        await SetCreatedAsync(id, new DateTime(2026, 3, 1, 17, 30, 0, DateTimeKind.Utc));
        var code = await CodeOfAsync(id);

        var res = await admin.GetAsync($"{Csv}?venueId={v.Id}");
        var bytes = await res.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
        Assert.Equal("text/csv", res.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", res.Content.Headers.ContentType?.CharSet);
        var stamp = D(Today).Replace("-", "");
        Assert.Equal($"bookings-{stamp}.csv", res.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        var text = Encoding.UTF8.GetString(bytes).TrimStart('﻿');
        Assert.EndsWith("\r\n", text);
        Assert.DoesNotContain("\n", text.Replace("\r\n", ""));
        var rows = ParseCsv(text);
        Assert.Equal("code,status,source,venue,customer,phone,payment,date,start,end,courts,total,commission,created_at", text.Split("\r\n")[0]);
        Assert.Equal(2, rows.Count);
        Assert.Equal(
            new[] { code, "confirmed", "app", "Schedule Test", "Somchai", "081-***-5678", "paid", D(Future), "10:00", "13:00", "คอร์ท A + คอร์ท B", "800.00", "80.00", "2026-03-02 00:30" },
            rows[1]);
    }

    [Fact]
    public async Task Csv_quotes_commas_quotes_and_newlines_and_round_trips_the_name()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var names = new[] { "Smith, John", "Say \"hi\"", "line1\nline2", "crlf1\r\nline2", "plain" };
        for (var i = 0; i < names.Length; i++)
            await v.AddBookingAsync(v.CourtA, Future, [$"{6 + i:00}:00"], source: "walk_in", customerName: names[i]);

        var (raw, res) = await CsvAsync(admin, $"?venueId={v.Id}");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("\"Smith, John\"", raw);
        Assert.Contains("\"Say \"\"hi\"\"\"", raw);
        Assert.Contains("\"line1\nline2\"", raw);
        Assert.Contains(",plain,", raw); // unquoted when nothing needs quoting
        var rows = CsvRows(raw);
        Assert.Equal(names.Length + 1, rows.Count);
        Assert.All(rows, r => Assert.Equal(14, r.Count));
        Assert.Equal(names.OrderBy(x => x), rows.Skip(1).Select(r => r[4]).OrderBy(x => x));
    }

    [Theory]
    [InlineData("=HYPERLINK(\"http://evil\")")]
    [InlineData("+1+1")]
    [InlineData("-2+3")]
    [InlineData("@SUM(A1)")]
    [InlineData("\tTabbed")]
    [InlineData("=a,b")]
    public async Task Csv_neutralises_formula_injection_in_names(string name)
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], source: "walk_in", customerName: name);

        var (raw, _) = await CsvAsync(admin, $"?venueId={v.Id}");

        var cell = CsvRows(raw)[1][4];
        Assert.Equal("'" + name, cell);
        Assert.DoesNotContain(CsvRows(raw)[1], c => c.Length > 0 && "=+-@\t\r".Contains(c[0]));
    }

    [Fact]
    public async Task Csv_leaves_a_name_with_a_formula_char_in_the_middle_alone()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], source: "walk_in", customerName: "a=b+c-d@e");

        var (raw, _) = await CsvAsync(admin, $"?venueId={v.Id}");

        Assert.Equal("a=b+c-d@e", CsvRows(raw)[1][4]);
    }

    [Fact]
    public async Task Csv_row_count_equals_the_json_total_for_the_same_filter()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var d = Future;
        for (var i = 0; i < 30; i++) // more than the default page of 25
            await v.AddBookingAsync(v.CourtA, d.AddDays(i % 3), [$"{6 + i / 3:00}:00"], status: i % 2 == 0 ? "confirmed" : "completed",
                source: i % 5 == 0 ? "walk_in" : "app", customerName: i % 5 == 0 ? $"W{i}" : null,
                userDisplayName: i % 5 == 0 ? null : $"U{i}", userPhone: i % 5 == 0 ? null : Phone("083", i.ToString("D4")));

        foreach (var q in new[]
        {
            $"venueId={v.Id}", $"venueId={v.Id}&status=confirmed", $"venueId={v.Id}&source=walk_in", $"venueId={v.Id}&from={D(d.AddDays(1))}",
            $"venueId={v.Id}&from={D(d)}&to={D(d)}&status=completed", $"venueId={v.Id}&q=u1", $"venueId={v.Id}&q=zzz-none",
        })
        {
            var total = (await GetAsync(admin, "?" + q + "&pageSize=1")).GetProperty("total").GetInt32();
            var (raw, res) = await CsvAsync(admin, "?" + q);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.Equal(total, CsvRows(raw).Count - 1);
        }
        Assert.Equal(30, CsvRows((await CsvAsync(admin, $"?venueId={v.Id}")).Raw).Count - 1);
    }

    [Fact]
    public async Task Csv_rows_come_out_in_the_same_order_as_the_json_list()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        var t = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        for (var i = 0; i < 6; i++)
            await SetCreatedAsync(await v.AddBookingAsync(v.CourtA, Future.AddDays(i), ["10:00"]), t.AddHours(i * 7 % 6));

        var json = Codes(await GetAsync(admin, $"?venueId={v.Id}"));
        var csv = CsvRows((await CsvAsync(admin, $"?venueId={v.Id}")).Raw).Skip(1).Select(r => r[0]).ToArray();

        Assert.Equal(json, csv);
    }

    [Fact]
    public async Task Csv_of_an_empty_result_is_just_the_header()
    {
        var admin = await AdminAsync();

        var (raw, res) = await CsvAsync(admin, $"?venueId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("﻿code,status,source,venue,customer,phone,payment,date,start,end,courts,total,commission,created_at\r\n", raw);
    }

    [Fact]
    public async Task Csv_refuses_more_than_20000_rows_but_allows_exactly_20000()
    {
        var v = await NewVenueAsync();
        var admin = await AdminAsync();
        // one set-based INSERT of slot-less bookings: cheap enough to test the real limit
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bookings (venue_id, status, source, customer_name, subtotal, total)
            SELECT {v.Id}, 'expired', 'walk_in', 'bulk ' || n, 0, 0 FROM generate_series(1, 20000) n
            """));

        var ok = await admin.GetAsync($"{Csv}?venueId={v.Id}");
        var okRaw = await ok.Content.ReadAsStringAsync();
        Assert.True(ok.StatusCode == HttpStatusCode.OK, okRaw.Length > 300 ? okRaw[..300] : okRaw);
        Assert.Equal(20_001, CsvRows(okRaw).Count);

        await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);

        var res = await admin.GetAsync($"{Csv}?venueId={v.Id}");
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.BadRequest, $"{(int)res.StatusCode}: {(raw.Length > 300 ? raw[..300] : raw)}");
        Assert.Equal("too_many_rows", JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
        // a narrower filter on the same data succeeds, and the JSON list is not limited by the CSV cap
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"{Csv}?venueId={v.Id}&q=bulk 19999")).StatusCode);
        Assert.Equal(20_001, (await GetAsync(admin, $"?venueId={v.Id}&pageSize=1")).GetProperty("total").GetInt32());
        Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));
    }
}
