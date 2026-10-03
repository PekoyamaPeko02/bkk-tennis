using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Admin;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// A4: weekly partner payouts (PayoutService + /admin/payouts). Every test builds its own partner(s) and venue(s) straight in the DB
/// so gross / commission / net are exact. The run settles ALL partners, so leftovers of other tests (e.g. completed app bookings on
/// the seeded partner) may be swept into payouts too: <see cref="RunAsync"/> subtracts those foreign partners from the counts, and
/// <see cref="DisposeAsync"/> unlinks + deletes every payout that did not exist before the test plus everything we created.
/// </summary>
[Collection(ApiCollection.Name)]
public class AdminPayoutsTests(ApiFactory api) : IAsyncLifetime
{
    private const string Base = "/api/v1/admin/payouts", AdminLogin = "/api/v1/auth/admin/login";

    private readonly List<Guid> _partners = [];
    private readonly List<Guid> _users = [];
    private Guid[] _payoutsBefore = [];
    private readonly int _errorsBefore = api.ServerErrors.Count;

    private DateOnly Today => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
    /// <summary>The Friday the default run would use, and the one before it (both are never in the future).</summary>
    private DateOnly F1 => PayoutService.LatestCutoff(Today);
    private DateOnly F0 => F1.AddDays(-7);
    private static DateTime Cut(DateOnly friday) => VenueClock.ToUtc(friday, TimeOnly.MinValue, Seed.Timezone);

    public async Task InitializeAsync() =>
        await api.WithDb(async db => _payoutsBefore = await db.Payouts.AsNoTracking().Select(p => p.Id).ToArrayAsync());

    public Task DisposeAsync() => api.WithDb(async db =>
    {
        var before = _payoutsBefore;
        // payouts this test caused for other partners (sweeps of foreign leftovers)
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET payout_id = NULL WHERE payout_id IN (SELECT id FROM payouts WHERE NOT (id = ANY({before})))");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE NOT (id = ANY({before}))");
        foreach (var p in _partners)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET payout_id = NULL WHERE payout_id IN (SELECT id FROM payouts WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM bookings WHERE venue_id IN (SELECT id FROM venues WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
        }
        foreach (var u in _users) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u}");
    });

    // ------------------------------------------------------------------ helpers

    private sealed record PV(Guid PartnerId, Guid VenueId, Guid CourtId);
    private sealed record Res(int Created, int Updated, int Skipped);

    private async Task<Guid> NewPartnerAsync(string? bank = "KBank", string? account = "123-4-56789-0")
    {
        var id = Guid.NewGuid();
        var legal = "Payout Partner " + id.ToString("N")[..8];
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
        var slug = "payout-" + v.ToString("N")[..12];
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({v}, {partner}, {slug}, 'ทดสอบ', {"Venue " + slug}, 'Test', 'x', 13.7, 100.5, 'hidden')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO courts (id, venue_id, name, surface, sort_order) VALUES ({c}, {v}, 'A', 'hard', 1)
                """);
        });
        return new PV(partner, v, c);
    }

    private async Task<PV> NewPartnerVenueAsync() => await NewVenueAsync(await NewPartnerAsync());

    /// <summary>
    /// Inserts a booking whose LAST slot ends at <paramref name="endUtc"/> (earlier slots are the hours before it).
    /// Slot status mirrors the booking status, like production; give active statuses (held/confirmed/checked_in) distinct ends
    /// at least 2 h apart on one court because of the exclusion constraint.
    /// </summary>
    private async Task<Guid> AddAsync(PV v, DateTime endUtc, decimal total, decimal commission, string status = "completed",
        string source = "app", int slots = 1)
    {
        var id = Guid.NewGuid();
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO bookings (id, venue_id, status, source, subtotal, addon_total, total, commission)
                VALUES ({id}, {v.VenueId}, {status}, {source}, {total}, 0, {total}, {commission})
                """);
            for (var i = 0; i < slots; i++)
            {
                var end = endUtc.AddHours(-(slots - 1 - i));
                var start = end.AddHours(-1);
                var price = total / slots;
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO booking_slots (booking_id, court_id, start_at, end_at, price, status)
                    VALUES ({id}, {v.CourtId}, {start}, {end}, {price}, {status})
                    """);
            }
        });
        return id;
    }

    /// <summary>A completed app booking without any slot: must never be settled.</summary>
    private async Task<Guid> AddNoSlotsAsync(PV v, decimal total, decimal commission)
    {
        var id = Guid.NewGuid();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bookings (id, venue_id, status, source, subtotal, addon_total, total, commission)
            VALUES ({id}, {v.VenueId}, 'completed', 'app', {total}, 0, {total}, {commission})
            """));
        return id;
    }

    /// <summary>Runs the service directly. Partners that are not ours and have something to settle are removed from Created.</summary>
    private async Task<Res> RunAsync(DateOnly cutoff)
    {
        var cutUtc = Cut(cutoff);
        var foreign = 0;
        await api.WithDb(async db =>
        {
            var partners = await db.Database.SqlQuery<Guid>($"""
                SELECT DISTINCT v.partner_id AS "Value" FROM bookings b JOIN venues v ON v.id = b.venue_id
                WHERE b.status = 'completed' AND b.source = 'app' AND b.payout_id IS NULL
                  AND EXISTS (SELECT 1 FROM booking_slots s WHERE s.booking_id = b.id)
                  AND (SELECT max(s.end_at) FROM booking_slots s WHERE s.booking_id = b.id) <= {cutUtc}
                """).ToListAsync();
            foreign = partners.Count(p => !_partners.Contains(p));
        });
        var r = await api.WithScope(sp => sp.GetRequiredService<PayoutService>().RunAsync(cutoff, CancellationToken.None));
        Assert.Equal(cutoff, r.Cutoff);
        return new Res(r.Created - foreign, r.Updated, r.Skipped);
    }

    private async Task<HttpClient> AdminAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync(AdminLogin, new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private void AssertNoServerErrors() =>
        Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));

    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd");

    private async Task<JsonElement> ReadAsync(HttpResponseMessage res, HttpStatusCode expected)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"expected {(int)expected} got {(int)res.StatusCode}: {raw}\n{string.Join("\n", api.ServerErrors.Skip(_errorsBefore))}");
        return string.IsNullOrEmpty(raw) ? default : JsonDocument.Parse(raw).RootElement.Clone();
    }

    private async Task<JsonElement> ListAsync(HttpClient admin, string query, HttpStatusCode expected = HttpStatusCode.OK) =>
        await ReadAsync(await admin.GetAsync(Base + query), expected);

    private static JsonElement[] Items(JsonElement page) => page.GetProperty("items").EnumerateArray().ToArray();
    private static string Grosses(JsonElement page) => string.Join(",", Items(page).Select(i => i.GetProperty("gross").GetDecimal().ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)));

    private async Task<Api.Domain.Payout?> PayoutAsync(Guid partner, DateOnly cutoff)
    {
        var start = cutoff.AddDays(-7);
        Api.Domain.Payout? p = null;
        await api.WithDb(async db => p = await db.Payouts.AsNoTracking().SingleOrDefaultAsync(x => x.PartnerId == partner && x.PeriodStart == start));
        return p;
    }

    private async Task<Api.Domain.Payout[]> PayoutsOfAsync(Guid partner)
    {
        Api.Domain.Payout[] r = [];
        await api.WithDb(async db => r = await db.Payouts.AsNoTracking().Where(x => x.PartnerId == partner).OrderBy(x => x.PeriodStart).ToArrayAsync());
        return r;
    }

    private async Task<Guid?> PayoutIdOfBookingAsync(Guid booking)
    {
        Guid? id = null;
        await api.WithDb(async db => id = (await db.Database.SqlQuery<Guid?>($"SELECT payout_id AS \"Value\" FROM bookings WHERE id = {booking}").ToListAsync()).Single());
        return id;
    }

    private async Task<Api.Domain.Payout> AssertPayoutAsync(Guid partner, DateOnly cutoff, decimal gross, decimal commission, string status = "pending")
    {
        var p = await PayoutAsync(partner, cutoff);
        Assert.NotNull(p);
        Assert.Equal(cutoff.AddDays(-7), p!.PeriodStart);
        Assert.Equal(cutoff.AddDays(-1), p.PeriodEnd);
        Assert.Equal(gross, p.Gross);
        Assert.Equal(commission, p.Commission);
        Assert.Equal(gross - commission, p.Net);
        Assert.Equal(status, p.Status);
        return p;
    }

    private async Task<int> BookingsInAsync(Guid payout)
    {
        var n = 0;
        await api.WithDb(async db => n = await db.Bookings.CountAsync(b => b.PayoutId == payout));
        return n;
    }

    // ------------------------------------------------------------------ authorization

    private static (HttpMethod, string, string?)[] Routes(Guid id) =>
    [
        (HttpMethod.Get, Base, null),
        (HttpMethod.Get, $"{Base}/{id}", null),
        (HttpMethod.Post, $"{Base}/run", null),
        (HttpMethod.Post, $"{Base}/{id}/mark-paid", "{\"transferRef\":\"X1\"}"),
    ];

    private static HttpRequestMessage Req((HttpMethod M, string Url, string? Body) r) =>
        new(r.M, r.Url) { Content = r.Body is null ? null : new StringContent(r.Body, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task Every_route_requires_a_token()
    {
        foreach (var r in Routes(Guid.NewGuid()))
        {
            var body = await ReadAsync(await api.Anonymous().SendAsync(Req(r)), HttpStatusCode.Unauthorized);
            Assert.Equal("unauthorized", body.GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Every_route_is_forbidden_for_players_and_partners_and_changes_nothing()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddHours(-30), 500, 50);
        var player = await api.LoginAsync();
        var owner = await api.LoginPartnerAsync(pv.VenueId, "owner", "partner_owner");
        var staff = await api.LoginPartnerAsync(pv.VenueId, "staff", "partner_staff");
        _users.AddRange([player.Id, owner.Id, staff.Id]);
        foreach (var u in new[] { player, owner, staff })
            foreach (var r in Routes(Guid.NewGuid()))
            {
                var body = await ReadAsync(await u.Http.SendAsync(Req(r)), HttpStatusCode.Forbidden);
                Assert.Equal("forbidden", body.GetProperty("code").GetString());
            }
        // the run route in particular must not have run
        Assert.Empty(await PayoutsOfAsync(pv.PartnerId));
    }

    // ------------------------------------------------------------------ settlement maths

    [Fact]
    public async Task One_payout_per_partner_sums_gross_commission_and_net_across_all_its_venues()
    {
        var partner = await NewPartnerAsync();
        var v1 = await NewVenueAsync(partner);
        var v2 = await NewVenueAsync(partner);
        var end = Cut(F1).AddDays(-2);
        var b1 = await AddAsync(v1, end, 800, 80, slots: 2);
        var b2 = await AddAsync(v1, end.AddHours(3), 1000.50m, 100.05m);
        var b3 = await AddAsync(v2, end, 450, 45);
        var b4 = await AddAsync(v2, end.AddHours(5), 1200, 120, slots: 3);

        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));

        var p = await AssertPayoutAsync(partner, F1, 3450.50m, 345.05m);
        Assert.Null(p.TransferRef);
        Assert.Null(p.PaidAt);
        Assert.Equal(4, await BookingsInAsync(p.Id));
        foreach (var b in new[] { b1, b2, b3, b4 }) Assert.Equal(p.Id, await PayoutIdOfBookingAsync(b));
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Two_partners_get_two_separate_payouts_with_their_own_totals()
    {
        var a = await NewPartnerVenueAsync();
        var b = await NewPartnerVenueAsync();
        var end = Cut(F1).AddDays(-3);
        await AddAsync(a, end, 600, 60);
        await AddAsync(a, end.AddHours(3), 400, 40);
        await AddAsync(b, end, 900, 90);

        Assert.Equal(new Res(2, 0, 0), await RunAsync(F1));

        var pa = await AssertPayoutAsync(a.PartnerId, F1, 1000, 100);
        var pb = await AssertPayoutAsync(b.PartnerId, F1, 900, 90);
        Assert.NotEqual(pa.Id, pb.Id);
        Assert.Equal(2, await BookingsInAsync(pa.Id));
        Assert.Equal(1, await BookingsInAsync(pb.Id));
    }

    [Fact]
    public async Task Only_completed_and_no_show_app_bookings_with_slots_are_settled()
    {
        var pv = await NewPartnerVenueAsync();
        var baseEnd = Cut(F1).AddDays(-3);
        var ineligible = new List<Guid>();
        var i = 0;
        // distinct, 2 h apart ends so held/confirmed/checked_in slots never overlap on the one court
        foreach (var status in new[] { "held", "confirmed", "checked_in", "cancelled", "expired" })
            ineligible.Add(await AddAsync(pv, baseEnd.AddHours(2 * i++), 700, 70, status));
        ineligible.Add(await AddAsync(pv, baseEnd.AddHours(2 * i++), 700, 0, "completed", "walk_in"));
        ineligible.Add(await AddNoSlotsAsync(pv, 700, 70));
        var good = await AddAsync(pv, baseEnd.AddHours(2 * i++), 300, 30);
        var noShow = await AddAsync(pv, baseEnd.AddHours(2 * i), 500, 50, "no_show"); // F1: a paid online no-show is revenue

        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));

        var p = await AssertPayoutAsync(pv.PartnerId, F1, 800, 80);
        Assert.Equal(2, await BookingsInAsync(p.Id));
        Assert.Equal(p.Id, await PayoutIdOfBookingAsync(good));
        Assert.Equal(p.Id, await PayoutIdOfBookingAsync(noShow));
        foreach (var id in ineligible) Assert.Null(await PayoutIdOfBookingAsync(id));
    }

    [Fact]
    public async Task A_partner_with_nothing_eligible_gets_no_payout_and_no_zero_row()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), 500, 50, "cancelled");
        await AddAsync(pv, Cut(F1).AddDays(-2).AddHours(3), 500, 0, "completed", "walk_in");

        Assert.Equal(new Res(0, 0, 0), await RunAsync(F1));

        Assert.Empty(await PayoutsOfAsync(pv.PartnerId));
    }

    // ------------------------------------------------------------------ cutoff boundary

    [Fact]
    public async Task Cutoff_boundary_is_midnight_Bangkok_inclusive_and_a_later_end_waits_for_the_next_Friday()
    {
        var pv = await NewPartnerVenueAsync();
        var cut = Cut(F0);
        Assert.Equal(17, cut.Hour); // Friday 00:00 Bangkok = Thursday 17:00 UTC
        Assert.Equal(DayOfWeek.Thursday, cut.DayOfWeek);
        var exactly = await AddAsync(pv, cut, 100, 10);                        // ends exactly at the cutoff: in
        var justBefore = await AddAsync(pv, cut.AddMinutes(-1).AddHours(-3), 200, 20);   // in
        var oneMinuteLate = await AddAsync(pv, cut.AddMinutes(1), 400, 40);    // out (this week)
        // first slot is before the cutoff but the LAST slot ends after it: out
        var straddling = await AddAsync(pv, cut.AddHours(1), 800, 80, slots: 3);

        Assert.Equal(new Res(1, 0, 0), await RunAsync(F0));

        var p0 = await AssertPayoutAsync(pv.PartnerId, F0, 300, 30);
        Assert.Equal(p0.Id, await PayoutIdOfBookingAsync(exactly));
        Assert.Equal(p0.Id, await PayoutIdOfBookingAsync(justBefore));
        Assert.Null(await PayoutIdOfBookingAsync(oneMinuteLate));
        Assert.Null(await PayoutIdOfBookingAsync(straddling));

        // next Friday's run picks them up in the following week's payout, never the earlier one again
        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));
        var p1 = await AssertPayoutAsync(pv.PartnerId, F1, 1200, 120);
        Assert.NotEqual(p0.Id, p1.Id);
        Assert.Equal(p1.Id, await PayoutIdOfBookingAsync(oneMinuteLate));
        Assert.Equal(p1.Id, await PayoutIdOfBookingAsync(straddling));
        await AssertPayoutAsync(pv.PartnerId, F0, 300, 30); // untouched
    }

    // ------------------------------------------------------------------ idempotency / merge / paid

    [Fact]
    public async Task Running_twice_changes_nothing_and_never_double_counts()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), 1000, 100);
        await AddAsync(pv, Cut(F1).AddDays(-2).AddHours(3), 500, 50);
        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));
        var first = await AssertPayoutAsync(pv.PartnerId, F1, 1500, 150);

        Assert.Equal(new Res(0, 0, 0), await RunAsync(F1));
        Assert.Equal(new Res(0, 0, 0), await RunAsync(F1));
        // a later cutoff has nothing new either
        Assert.Equal(new Res(0, 0, 0), await RunAsync(F1));

        var all = await PayoutsOfAsync(pv.PartnerId);
        var only = Assert.Single(all);
        Assert.Equal(first.Id, only.Id);
        Assert.Equal(1500, only.Gross);
        Assert.Equal(150, only.Commission);
        Assert.Equal(1350, only.Net);
        Assert.Equal(first.CreatedAt, only.CreatedAt);
        Assert.Equal(2, await BookingsInAsync(first.Id));
    }

    [Fact]
    public async Task A_late_completion_merges_into_the_weeks_pending_payout_and_recomputes_totals()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), 1000, 100);
        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));
        var p = await AssertPayoutAsync(pv.PartnerId, F1, 1000, 100);

        // completes late (e.g. check-in + settle after the first run) but its slot ended before the cutoff
        var late = await AddAsync(pv, Cut(F1).AddDays(-4), 250.75m, 25.08m);
        Assert.Equal(new Res(0, 1, 0), await RunAsync(F1));

        var merged = await AssertPayoutAsync(pv.PartnerId, F1, 1250.75m, 125.08m);
        Assert.Equal(p.Id, merged.Id);
        Assert.Equal(p.CreatedAt, merged.CreatedAt);
        Assert.Equal(p.Id, await PayoutIdOfBookingAsync(late));
        Assert.Equal(2, await BookingsInAsync(p.Id));
        Assert.Single(await PayoutsOfAsync(pv.PartnerId));

        Assert.Equal(new Res(0, 0, 0), await RunAsync(F1)); // and now stable again
    }

    [Fact]
    public async Task A_paid_payout_is_never_touched_and_new_bookings_roll_into_the_next_weeks_payout()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F0).AddDays(-2), 1000, 100);
        Assert.Equal(new Res(1, 0, 0), await RunAsync(F0));
        var p0 = await AssertPayoutAsync(pv.PartnerId, F0, 1000, 100);
        var admin = await AdminAsync();
        await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p0.Id}/mark-paid", new { transferRef = "TR-1" }), HttpStatusCode.OK);

        var rolled = await AddAsync(pv, Cut(F0).AddDays(-3), 600, 60);
        Assert.Equal(new Res(0, 0, 1), await RunAsync(F0)); // skipped
        await AssertPayoutAsync(pv.PartnerId, F0, 1000, 100, "paid"); // totals frozen
        Assert.Null(await PayoutIdOfBookingAsync(rolled));

        Assert.Equal(new Res(1, 0, 0), await RunAsync(F1));
        var p1 = await AssertPayoutAsync(pv.PartnerId, F1, 600, 60);
        Assert.Equal(p1.Id, await PayoutIdOfBookingAsync(rolled));
        Assert.Equal(1, await BookingsInAsync(p0.Id));
    }

    [Fact]
    public async Task A_failed_payout_is_skipped_too_until_someone_marks_it_paid()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), 1000, 100);
        await RunAsync(F1);
        var p = await AssertPayoutAsync(pv.PartnerId, F1, 1000, 100);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE payouts SET status = 'failed' WHERE id = {p.Id}"));
        var extra = await AddAsync(pv, Cut(F1).AddDays(-3), 100, 10);

        Assert.Equal(new Res(0, 0, 1), await RunAsync(F1));

        await AssertPayoutAsync(pv.PartnerId, F1, 1000, 100, "failed");
        Assert.Null(await PayoutIdOfBookingAsync(extra));
    }

    // ------------------------------------------------------------------ concurrency

    [Fact]
    public async Task Parallel_service_runs_count_every_booking_exactly_once_with_one_payout_per_partner()
    {
        var partners = new List<(PV V1, PV V2, List<Guid> Ids)>();
        for (var k = 0; k < 3; k++)
        {
            var p = await NewPartnerAsync();
            var v1 = await NewVenueAsync(p);
            var v2 = await NewVenueAsync(p);
            var ids = new List<Guid>();
            for (var j = 0; j < 6; j++)
                ids.Add(await AddAsync(j % 2 == 0 ? v1 : v2, Cut(F1).AddDays(-2).AddHours(2 * j), 100 * (j + 1) + k, 10 * (j + 1)));
            partners.Add((v1, v2, ids));
        }

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ =>
            Task.Run(() => api.WithScope(sp => sp.GetRequiredService<PayoutService>().RunAsync(F1, CancellationToken.None)))));

        Assert.All(results, r => Assert.Equal(F1, r.Cutoff));
        foreach (var (v1, _, ids) in partners)
        {
            var mine = await PayoutsOfAsync(v1.PartnerId);
            var payout = Assert.Single(mine);
            Assert.Equal(F1.AddDays(-7), payout.PeriodStart);
            Assert.Equal("pending", payout.Status);
            var expectedGross = 0m;
            var expectedCommission = 0m;
            await api.WithDb(async db =>
            {
                var bs = await db.Bookings.AsNoTracking().Where(b => ids.Contains(b.Id)).ToListAsync();
                Assert.All(bs, b => Assert.Equal(payout.Id, b.PayoutId));
                expectedGross = bs.Sum(b => b.Total);
                expectedCommission = bs.Sum(b => b.Commission);
            });
            Assert.Equal(6, await BookingsInAsync(payout.Id));
            Assert.Equal(expectedGross, payout.Gross);
            Assert.Equal(expectedCommission, payout.Commission);
            Assert.Equal(expectedGross - expectedCommission, payout.Net);
        }
        // across the whole DB the payout totals of ours equal the bookings' truth (no booking in two payouts, none lost)
        var ours = partners.Select(p => p.V1.PartnerId).ToArray();
        await api.WithDb(async db =>
        {
            var gross = await db.Payouts.Where(p => ours.Contains(p.PartnerId)).SumAsync(p => p.Gross);
            var linked = await db.Bookings.Where(b => b.PayoutId != null && ours.Contains(b.Venue!.PartnerId)).SumAsync(b => b.Total);
            Assert.Equal(linked, gross);
        });
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Parallel_POST_run_calls_never_500_and_leave_consistent_payouts()
    {
        var admin = await AdminAsync();
        var pvs = new List<PV>();
        for (var k = 0; k < 4; k++)
        {
            var pv = await NewPartnerVenueAsync();
            await AddAsync(pv, Cut(F1).AddDays(-2), 500 + k, 50);
            await AddAsync(pv, Cut(F1).AddDays(-2).AddHours(3), 300, 30);
            pvs.Add(pv);
        }

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ =>
            Task.Run(() => admin.PostAsync($"{Base}/run?cutoff={D(F1)}", null))));

        foreach (var r in responses) await ReadAsync(r, HttpStatusCode.OK);
        for (var k = 0; k < pvs.Count; k++)
        {
            var p = Assert.Single(await PayoutsOfAsync(pvs[k].PartnerId));
            Assert.Equal(800m + k, p.Gross);
            Assert.Equal(80m, p.Commission);
            Assert.Equal(720m + k, p.Net);
            Assert.Equal(2, await BookingsInAsync(p.Id));
        }
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Parallel_runs_racing_with_late_completions_still_count_each_booking_once()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), 100, 10);
        var runs = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
            api.WithScope(sp => sp.GetRequiredService<PayoutService>().RunAsync(F1, CancellationToken.None)))).ToList();
        var late = await AddAsync(pv, Cut(F1).AddDays(-3), 40, 4);
        runs.AddRange(Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
            api.WithScope(sp => sp.GetRequiredService<PayoutService>().RunAsync(F1, CancellationToken.None)))));
        await Task.WhenAll(runs);
        await RunAsync(F1); // settle whatever the race left behind

        var p = Assert.Single(await PayoutsOfAsync(pv.PartnerId));
        Assert.Equal(140, p.Gross);
        Assert.Equal(14, p.Commission);
        Assert.Equal(126, p.Net);
        Assert.Equal(2, await BookingsInAsync(p.Id));
        Assert.Equal(p.Id, await PayoutIdOfBookingAsync(late));
        AssertNoServerErrors();
    }

    // ------------------------------------------------------------------ end to end with real check-in and settle

    [Fact]
    public async Task Booking_checked_in_and_settled_by_the_real_endpoints_ends_up_in_the_Friday_payout()
    {
        var pv = await NewPartnerVenueAsync();
        // The fake clock is shared and only moves forward (AdminTests compares "today" against DB now()), so keep the jump to
        // ~1 h: the game is in progress right now, gets checked in, ends, is settled by the real BookingService; only then the
        // finished game is moved back in time (SQL) so that it lies before the latest Friday's cutoff.
        var start = api.Clock.GetUtcNow().UtcDateTime.AddMinutes(-10);
        var id = await AddAsync(pv, start.AddHours(1), 400, 40, "confirmed");

        var staff = await api.LoginPartnerAsync(pv.VenueId);
        _users.Add(staff.Id);
        await ReadAsync(await staff.Http.PostAsync($"/api/v1/clubos/venues/{pv.VenueId}/bookings/{id}/check-in", null), HttpStatusCode.OK);

        api.Clock.Advance(TimeSpan.FromMinutes(61));
        await api.WithScope(sp => sp.GetRequiredService<BookingService>().SettleBookingsAsync(CancellationToken.None));
        await api.WithDb(async db => Assert.Equal("completed", (await db.Database.SqlQuery<string>($"SELECT status AS \"Value\" FROM bookings WHERE id = {id}").ToListAsync()).Single()));
        Assert.Null(await PayoutIdOfBookingAsync(id)); // completed but not settled yet

        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE booking_slots SET start_at = start_at - interval '10 days', end_at = end_at - interval '10 days' WHERE booking_id = {id}"));

        var admin = await AdminAsync();
        var run = await ReadAsync(await admin.PostAsync($"{Base}/run", null), HttpStatusCode.OK);
        Assert.Equal(D(F1), run.GetProperty("cutoff").GetString());
        var p = await AssertPayoutAsync(pv.PartnerId, F1, 400, 40);
        Assert.Equal(p.Id, await PayoutIdOfBookingAsync(id));
    }

    // ------------------------------------------------------------------ mark-paid

    private async Task<(PV Pv, Api.Domain.Payout P)> PendingPayoutAsync(decimal total = 1000, decimal commission = 100)
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddDays(-2), total, commission);
        await RunAsync(F1);
        return (pv, (await PayoutAsync(pv.PartnerId, F1))!);
    }

    [Fact]
    public async Task Mark_paid_stores_the_trimmed_reference_and_the_clock_time()
    {
        var (_, p) = await PendingPayoutAsync();
        var admin = await AdminAsync();

        var dto = await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = "  SCB-20260925-001  " }), HttpStatusCode.OK);

        Assert.Equal(p.Id, dto.GetProperty("id").GetGuid());
        Assert.Equal("paid", dto.GetProperty("status").GetString());
        Assert.Equal("SCB-20260925-001", dto.GetProperty("transferRef").GetString());
        var paidAt = dto.GetProperty("paidAt").GetDateTimeOffset();
        Assert.True(Math.Abs((paidAt - api.Clock.GetUtcNow()).TotalSeconds) < 1, $"paidAt {paidAt} vs clock {api.Clock.GetUtcNow()}");
        Assert.Equal(1000m, dto.GetProperty("gross").GetDecimal());
        var db1 = await PayoutAsync(p.PartnerId, F1);
        Assert.Equal("paid", db1!.Status);
        Assert.Equal("SCB-20260925-001", db1.TransferRef);
        Assert.NotNull(db1.PaidAt);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Marking_an_already_paid_payout_is_409_bad_transition_and_keeps_the_first_reference()
    {
        var (_, p) = await PendingPayoutAsync();
        var admin = await AdminAsync();
        await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = "FIRST" }), HttpStatusCode.OK);
        var first = await PayoutAsync(p.PartnerId, F1);
        api.Clock.Advance(TimeSpan.FromMinutes(5));

        var body = await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = "SECOND" }), HttpStatusCode.Conflict);

        Assert.Equal("bad_transition", body.GetProperty("code").GetString());
        var after = await PayoutAsync(p.PartnerId, F1);
        Assert.Equal("FIRST", after!.TransferRef);
        Assert.Equal(first!.PaidAt, after.PaidAt);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Parallel_mark_paid_lets_exactly_one_win_and_the_winners_reference_sticks()
    {
        var (_, p) = await PendingPayoutAsync();
        var admin = await AdminAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(i =>
            Task.Run(async () => (Ref: "REF-" + i, Res: await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = "REF-" + i })))));

        var codes = responses.Select(r => r.Res.StatusCode).ToArray();
        Assert.True(codes.Count(c => c == HttpStatusCode.OK) == 1 && codes.Count(c => c == HttpStatusCode.Conflict) == 7, string.Join(",", codes));
        var winner = responses.Single(r => r.Res.StatusCode == HttpStatusCode.OK).Ref;
        Assert.Equal(winner, (await PayoutAsync(p.PartnerId, F1))!.TransferRef);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task A_failed_payout_can_be_marked_paid()
    {
        var (_, p) = await PendingPayoutAsync();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE payouts SET status = 'failed' WHERE id = {p.Id}"));
        var admin = await AdminAsync();

        var dto = await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = "RETRY-OK" }), HttpStatusCode.OK);

        Assert.Equal("paid", dto.GetProperty("status").GetString());
        Assert.Equal("RETRY-OK", dto.GetProperty("transferRef").GetString());
    }

    [Fact]
    public async Task Mark_paid_on_an_unknown_payout_is_404()
    {
        var admin = await AdminAsync();
        var body = await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{Guid.NewGuid()}/mark-paid", new { transferRef = "X" }), HttpStatusCode.NotFound);
        Assert.Equal("not_found", body.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"transferRef\":null}")]
    [InlineData("{\"transferRef\":\"\"}")]
    [InlineData("{\"transferRef\":\"   \"}")]
    [InlineData("{\"transferRef\":\"x\"}", true)] // 81 chars is patched in below
    public async Task Mark_paid_rejects_a_missing_blank_or_too_long_reference_and_the_payout_stays_pending(string json, bool tooLong = false)
    {
        var (_, p) = await PendingPayoutAsync();
        var admin = await AdminAsync();
        if (tooLong) json = JsonSerializer.Serialize(new { transferRef = new string('r', 81) });

        var res = await admin.PostAsync($"{Base}/{p.Id}/mark-paid", new StringContent(json, Encoding.UTF8, "application/json"));

        var body = await ReadAsync(res, HttpStatusCode.BadRequest);
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.Equal("pending", (await PayoutAsync(p.PartnerId, F1))!.Status);
    }

    [Fact]
    public async Task Mark_paid_accepts_exactly_80_characters()
    {
        var (_, p) = await PendingPayoutAsync();
        var admin = await AdminAsync();
        var r = new string('z', 80);
        var dto = await ReadAsync(await admin.PostAsJsonAsync($"{Base}/{p.Id}/mark-paid", new { transferRef = r }), HttpStatusCode.OK);
        Assert.Equal(r, dto.GetProperty("transferRef").GetString());
    }

    // ------------------------------------------------------------------ list / detail

    private async Task InsertPayoutAsync(Guid partner, DateOnly cutoff, string status, decimal gross = 100, decimal commission = 10, DateTime? created = null)
    {
        var id = Guid.NewGuid();
        var start = cutoff.AddDays(-7);
        var end = cutoff.AddDays(-1);
        var net = gross - commission;
        var at = created ?? api.Clock.GetUtcNow().UtcDateTime;
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO payouts (id, partner_id, period_start, period_end, gross, commission, net, status, created_at)
            VALUES ({id}, {partner}, {start}, {end}, {gross}, {commission}, {net}, {status}, {at})
            """));
    }

    [Fact]
    public async Task List_shape_carries_partner_name_bank_details_dates_and_booking_count()
    {
        var partner = await NewPartnerAsync("SCB", "999-9-99999-9");
        var v = await NewVenueAsync(partner);
        await AddAsync(v, Cut(F1).AddDays(-2), 700, 70);
        await AddAsync(v, Cut(F1).AddDays(-2).AddHours(3), 300, 30);
        await RunAsync(F1);
        var admin = await AdminAsync();

        var page = await ListAsync(admin, $"?partnerId={partner}");

        Assert.Equal(1, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(25, page.GetProperty("pageSize").GetInt32());
        var i = Assert.Single(Items(page));
        Assert.Equal(partner, i.GetProperty("partnerId").GetGuid());
        Assert.StartsWith("Payout Partner ", i.GetProperty("partnerName").GetString());
        Assert.Equal("SCB", i.GetProperty("bankName").GetString());
        Assert.Equal("999-9-99999-9", i.GetProperty("bankAccount").GetString());
        Assert.Equal(D(F1.AddDays(-7)), i.GetProperty("periodStart").GetString());
        Assert.Equal(D(F1.AddDays(-1)), i.GetProperty("periodEnd").GetString());
        Assert.Equal(2, i.GetProperty("bookingCount").GetInt32());
        Assert.Equal(1000m, i.GetProperty("gross").GetDecimal());
        Assert.Equal(100m, i.GetProperty("commission").GetDecimal());
        Assert.Equal(900m, i.GetProperty("net").GetDecimal());
        Assert.Equal("pending", i.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, i.GetProperty("transferRef").ValueKind);
        Assert.Equal(JsonValueKind.Null, i.GetProperty("paidAt").ValueKind);
    }

    [Fact]
    public async Task List_filters_by_status_and_partner_and_orders_by_period_then_created()
    {
        var partner = await NewPartnerAsync();
        var other = await NewPartnerAsync();
        await InsertPayoutAsync(partner, F1.AddDays(-14), "paid", 10, 1);
        await InsertPayoutAsync(partner, F1, "pending", 30, 3);
        await InsertPayoutAsync(partner, F1.AddDays(-7), "failed", 20, 2);
        await InsertPayoutAsync(partner, F1.AddDays(-21), "paid", 5, 0.5m);
        await InsertPayoutAsync(other, F1, "pending", 99, 9);
        var admin = await AdminAsync();

        var all = await ListAsync(admin, $"?partnerId={partner}");
        Assert.Equal(4, all.GetProperty("total").GetInt32());
        Assert.Equal("30,20,10,5", Grosses(all)); // period_start desc
        Assert.All(Items(all), i => Assert.Equal(partner, i.GetProperty("partnerId").GetGuid()));

        var paid = await ListAsync(admin, $"?partnerId={partner}&status=paid");
        Assert.Equal(2, paid.GetProperty("total").GetInt32());
        Assert.All(Items(paid), i => Assert.Equal("paid", i.GetProperty("status").GetString()));
        var pending = await ListAsync(admin, $"?partnerId={partner}&status=pending");
        Assert.Equal("30", Grosses(pending));
        var failed = await ListAsync(admin, $"?partnerId={partner}&status=failed");
        Assert.Equal("20", Grosses(failed));
        var onlyOther = await ListAsync(admin, $"?partnerId={other}");
        Assert.Equal(1, onlyOther.GetProperty("total").GetInt32());
        var none = await ListAsync(admin, $"?partnerId={Guid.NewGuid()}");
        Assert.Equal(0, none.GetProperty("total").GetInt32());
        Assert.Empty(Items(none));
    }

    [Fact]
    public async Task List_paginates_with_stable_order_and_clamps_page_and_pageSize()
    {
        var partner = await NewPartnerAsync();
        for (var w = 0; w < 5; w++) await InsertPayoutAsync(partner, F1.AddDays(-7 * w), "pending", 100 + w, 1);
        var admin = await AdminAsync();

        var p1 = await ListAsync(admin, $"?partnerId={partner}&pageSize=2&page=1");
        var p2 = await ListAsync(admin, $"?partnerId={partner}&pageSize=2&page=2");
        var p3 = await ListAsync(admin, $"?partnerId={partner}&pageSize=2&page=3");
        var p4 = await ListAsync(admin, $"?partnerId={partner}&pageSize=2&page=4");
        Assert.Equal(5, p1.GetProperty("total").GetInt32());
        Assert.Equal("100,101", Grosses(p1));
        Assert.Equal("102,103", Grosses(p2));
        Assert.Equal("104", Grosses(p3));
        Assert.Empty(Items(p4));
        Assert.Equal(2, p2.GetProperty("page").GetInt32());

        Assert.Equal(1, (await ListAsync(admin, $"?partnerId={partner}&pageSize=0")).GetProperty("pageSize").GetInt32());
        Assert.Equal(100, (await ListAsync(admin, $"?partnerId={partner}&pageSize=1000")).GetProperty("pageSize").GetInt32());
        var pg0 = await ListAsync(admin, $"?partnerId={partner}&page=0");
        Assert.Equal(1, pg0.GetProperty("page").GetInt32());
        Assert.Equal(5, Items(pg0).Length);
    }

    [Fact]
    public async Task List_orders_equal_periods_by_newest_created_first()
    {
        // two partners, same week: the newer row comes first. Ordered against each other through the unfiltered list.
        var a = await NewPartnerAsync();
        var b = await NewPartnerAsync();
        var t = api.Clock.GetUtcNow().UtcDateTime;
        await InsertPayoutAsync(a, F1, "pending", 1, 0, t.AddMinutes(-10));
        await InsertPayoutAsync(b, F1, "pending", 2, 0, t.AddMinutes(-5));
        var admin = await AdminAsync();

        var page = await ListAsync(admin, "?pageSize=100");
        var order = Items(page).Select(i => i.GetProperty("partnerId").GetGuid()).ToList();
        // there are only ever a handful of payouts (all our own): the newer one (b) must precede the older one (a)
        Assert.True(order.IndexOf(b) >= 0 && order.IndexOf(a) >= 0);
        Assert.True(order.IndexOf(b) < order.IndexOf(a), "equal period_start must sort created desc");
    }

    [Fact]
    public async Task List_rejects_an_unknown_status_with_400_bad_status()
    {
        var admin = await AdminAsync();
        foreach (var s in new[] { "PAID", "done", "" })
        {
            var body = await ReadAsync(await admin.GetAsync($"{Base}?status={s}"), HttpStatusCode.BadRequest);
            Assert.Equal("bad_status", body.GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Detail_lists_the_settled_bookings_with_venue_local_date_and_amounts_summing_to_the_payout()
    {
        var partner = await NewPartnerAsync();
        var v1 = await NewVenueAsync(partner);
        var v2 = await NewVenueAsync(partner);
        var thursdayNight = Cut(F1);                       // ends exactly at the cutoff, starts Thursday 23:00 local
        var b1 = await AddAsync(v1, thursdayNight, 600, 60);
        var d2 = F1.AddDays(-3);
        var b2 = await AddAsync(v2, ScheduleVenue.Utc(d2, "01:30"), 400, 40); // starts 00:30 local = previous day in UTC
        var b3 = await AddAsync(v2, Cut(F1).AddDays(-4), 250, 25, slots: 2);
        await RunAsync(F1);
        var p = (await PayoutAsync(partner, F1))!;
        var admin = await AdminAsync();

        var detail = await ReadAsync(await admin.GetAsync($"{Base}/{p.Id}"), HttpStatusCode.OK);

        var dto = detail.GetProperty("payout");
        Assert.Equal(p.Id, dto.GetProperty("id").GetGuid());
        Assert.Equal(3, dto.GetProperty("bookingCount").GetInt32());
        var rows = detail.GetProperty("bookings").EnumerateArray().ToArray();
        Assert.Equal(3, rows.Length);
        Assert.Equal(rows.Select(r => r.GetProperty("code").GetString()).OrderBy(c => c, StringComparer.Ordinal), rows.Select(r => r.GetProperty("code").GetString()));
        Assert.Equal(dto.GetProperty("gross").GetDecimal(), rows.Sum(r => r.GetProperty("total").GetDecimal()));
        Assert.Equal(dto.GetProperty("commission").GetDecimal(), rows.Sum(r => r.GetProperty("commission").GetDecimal()));
        Assert.Equal(new[] { b1, b2, b3 }.OrderBy(x => x), rows.Select(r => r.GetProperty("id").GetGuid()).OrderBy(x => x));

        var row1 = rows.Single(r => r.GetProperty("id").GetGuid() == b1);
        Assert.Equal(D(F1.AddDays(-1)), row1.GetProperty("date").GetString()); // Thursday, not Friday
        Assert.StartsWith("Venue payout-", row1.GetProperty("venueName").GetString());
        Assert.Equal(600m, row1.GetProperty("total").GetDecimal());
        Assert.Equal(60m, row1.GetProperty("commission").GetDecimal());
        var row2 = rows.Single(r => r.GetProperty("id").GetGuid() == b2);
        Assert.Equal(D(d2), row2.GetProperty("date").GetString());
    }

    [Fact]
    public async Task Detail_of_an_unknown_payout_is_404_with_a_json_body()
    {
        var admin = await AdminAsync();
        var body = await ReadAsync(await admin.GetAsync($"{Base}/{Guid.NewGuid()}"), HttpStatusCode.NotFound);
        Assert.Equal("not_found", body.GetProperty("code").GetString());
    }

    // ------------------------------------------------------------------ POST /run validation

    [Fact]
    public async Task Run_rejects_a_cutoff_that_is_not_a_Friday()
    {
        var admin = await AdminAsync();
        for (var d = 0; d < 7; d++)
        {
            var day = F0.AddDays(d);
            if (day.DayOfWeek == DayOfWeek.Friday) continue;
            var body = await ReadAsync(await admin.PostAsync($"{Base}/run?cutoff={D(day)}", null), HttpStatusCode.BadRequest);
            Assert.Equal("not_friday", body.GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Run_rejects_a_Friday_after_today_and_takes_precedence_after_the_weekday_check()
    {
        var admin = await AdminAsync();
        var body = await ReadAsync(await admin.PostAsync($"{Base}/run?cutoff={D(F1.AddDays(7))}", null), HttpStatusCode.BadRequest);
        Assert.Equal("future_cutoff", body.GetProperty("code").GetString());
        // a future non-Friday is reported as not_friday
        var body2 = await ReadAsync(await admin.PostAsync($"{Base}/run?cutoff={D(F1.AddDays(8))}", null), HttpStatusCode.BadRequest);
        Assert.Equal("not_friday", body2.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Run_rejects_a_malformed_cutoff_with_400()
    {
        var admin = await AdminAsync();
        foreach (var bad in new[] { "abc", "2026-02-30", "26-01-02", "2026-13-01" })
        {
            var body = await ReadAsync(await admin.PostAsync($"{Base}/run?cutoff={bad}", null), HttpStatusCode.BadRequest);
            Assert.True(body.TryGetProperty("code", out _), $"cutoff={bad}: 400 without the JSON error contract: {body}");
        }
        // (the framework's BadHttpRequestException is logged at Error by the exception handler: not asserted as a server error)
    }

    [Fact]
    public async Task Run_without_a_cutoff_uses_the_latest_Friday_and_reports_the_counts()
    {
        var pv = await NewPartnerVenueAsync();
        await AddAsync(pv, Cut(F1).AddHours(-5), 1000, 100);
        var admin = await AdminAsync();

        var first = await ReadAsync(await admin.PostAsync($"{Base}/run", null), HttpStatusCode.OK);

        Assert.Equal(D(F1), first.GetProperty("cutoff").GetString());
        Assert.True(first.GetProperty("created").GetInt32() >= 1);
        Assert.Equal(0, first.GetProperty("skipped").GetInt32());
        await AssertPayoutAsync(pv.PartnerId, F1, 1000, 100);
        var second = await ReadAsync(await admin.PostAsync($"{Base}/run", null), HttpStatusCode.OK);
        Assert.Equal(0, second.GetProperty("created").GetInt32());
        Assert.Equal(0, second.GetProperty("updated").GetInt32());
        Assert.Equal(0, second.GetProperty("skipped").GetInt32());
    }

    [Fact]
    public async Task Run_accepts_an_old_Friday_and_settles_only_what_ended_before_it()
    {
        var pv = await NewPartnerVenueAsync();
        var old = F1.AddDays(-28);
        await AddAsync(pv, Cut(old).AddDays(-2), 100, 10);
        await AddAsync(pv, Cut(F1).AddDays(-2), 900, 90);
        var admin = await AdminAsync();

        var res = await ReadAsync(await admin.PostAsync($"{Base}/run?cutoff={D(old)}", null), HttpStatusCode.OK);

        Assert.Equal(D(old), res.GetProperty("cutoff").GetString());
        await AssertPayoutAsync(pv.PartnerId, old, 100, 10);
        Assert.Null(await PayoutAsync(pv.PartnerId, F1));
    }

    // ------------------------------------------------------------------ pure helpers

    [Fact]
    public void IsPayoutDay_is_true_only_on_Fridays()
    {
        for (var d = new DateOnly(2024, 12, 20); d < new DateOnly(2026, 1, 15); d = d.AddDays(1))
            Assert.Equal(d.DayOfWeek == DayOfWeek.Friday, PayoutService.IsPayoutDay(d));
    }

    [Fact]
    public void LatestCutoff_is_the_most_recent_Friday_on_or_before_the_day_for_every_date()
    {
        for (var d = new DateOnly(2023, 12, 20); d < new DateOnly(2026, 1, 15); d = d.AddDays(1))
        {
            var c = PayoutService.LatestCutoff(d);
            Assert.Equal(DayOfWeek.Friday, c.DayOfWeek);
            Assert.True(c <= d, $"{d}: {c} is in the future");
            Assert.True(d.DayNumber - c.DayNumber <= 6, $"{d}: {c} is more than a week back");
        }
    }

    [Theory]
    [InlineData("2026-09-24", "2026-09-18")] // Thursday
    [InlineData("2026-09-25", "2026-09-25")] // Friday itself
    [InlineData("2026-09-26", "2026-09-25")] // Saturday
    [InlineData("2026-09-27", "2026-09-25")] // Sunday
    [InlineData("2026-09-28", "2026-09-25")] // Monday
    [InlineData("2026-09-29", "2026-09-25")] // Tuesday
    [InlineData("2026-09-30", "2026-09-25")] // Wednesday
    [InlineData("2026-01-01", "2025-12-26")] // year boundary (Thursday)
    [InlineData("2026-01-02", "2026-01-02")] // Friday on new year
    [InlineData("2025-03-01", "2025-02-28")] // month boundary (Saturday)
    [InlineData("2024-02-29", "2024-02-23")] // leap day (Thursday)
    [InlineData("2024-03-01", "2024-03-01")] // Friday right after a leap day
    [InlineData("2024-03-04", "2024-03-01")] // Monday: crosses back over the leap day
    [InlineData("2025-01-01", "2024-12-27")] // Wednesday
    public void LatestCutoff_known_dates(string today, string expected) =>
        Assert.Equal(DateOnly.Parse(expected), PayoutService.LatestCutoff(DateOnly.Parse(today)));

    [Theory]
    [InlineData("2026-09-24T16:59:59Z", "2026-09-24")] // Thursday 23:59:59 in Bangkok
    [InlineData("2026-09-24T17:00:00Z", "2026-09-25")] // Friday 00:00 in Bangkok
    [InlineData("2026-09-25T16:59:59Z", "2026-09-25")]
    [InlineData("2026-09-25T17:00:00Z", "2026-09-26")]
    public void Today_follows_the_Bangkok_calendar_not_UTC(string utc, string expected)
    {
        var svc = new PayoutService(null!, new FakeTimeProvider(DateTimeOffset.Parse(utc)), NullLogger<PayoutService>.Instance);
        Assert.Equal(DateOnly.Parse(expected), svc.Today);
        Assert.Equal(DateOnly.Parse(expected).DayOfWeek == DayOfWeek.Friday, PayoutService.IsPayoutDay(svc.Today));
    }
}
