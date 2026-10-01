using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Features.Admin;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// A non-default partner commission rate (set at onboarding) must reach bookings.commission, the admin overview take and the
/// weekly payout. Cleanup: everything created here, plus payouts a run swept for foreign partners (like AdminPayoutsTests).
/// </summary>
[Collection(ApiCollection.Name)]
public class AdminPartnerCommissionTests(ApiFactory api) : IAsyncLifetime
{
    private const string OwnerPw = "owner-pass-2026", Overview = "/api/v1/admin/overview";
    private const double Lat = 14.6, Lng = 101.3;

    private readonly List<Guid> _partners = [];
    private Guid[] _payoutsBefore = [];
    private readonly int _errorsBefore = api.ServerErrors.Count;

    public async Task InitializeAsync() =>
        await api.WithDb(async db => _payoutsBefore = await db.Payouts.AsNoTracking().Select(p => p.Id).ToArrayAsync());

    public Task DisposeAsync() => api.WithDb(async db =>
    {
        var before = _payoutsBefore;
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET payout_id = NULL WHERE payout_id IN (SELECT id FROM payouts WHERE NOT (id = ANY({before})))");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE NOT (id = ANY({before}))");
        foreach (var p in _partners)
        {
            var owners = await db.VenueMembers.Where(m => m.Role == "owner" && db.Venues.Any(v => v.Id == m.VenueId && v.PartnerId == p))
                .Select(m => m.UserId).ToListAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM refunds WHERE payment_id IN (SELECT pay.id FROM payments pay JOIN bookings b ON b.id = pay.booking_id JOIN venues v ON v.id = b.venue_id WHERE v.partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payments WHERE booking_id IN (SELECT b.id FROM bookings b JOIN venues v ON v.id = b.venue_id WHERE v.partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM bookings WHERE venue_id IN (SELECT id FROM venues WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
            foreach (var u in owners) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u} AND role = 'partner_owner'");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
        }
    });

    private async Task<HttpClient> LoginAsync(string url, object body)
    {
        var res = await api.Anonymous().PostAsJsonAsync(url, body);
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage res, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"expected {(int)expected} got {(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement.Clone();
    }

    [Theory]
    [InlineData(0.15, 45)]
    [InlineData(0, 0)]
    public async Task Custom_partner_commission_rate_reaches_booking_overview_take_and_payout(double rate, int expectedCommission)
    {
        var tag = Guid.NewGuid().ToString("N")[..10];
        var email = $"owner-{tag}@example.test";
        var admin = await LoginAsync("/api/v1/auth/admin/login", new { email = "admin@bkktennis.test", password = "admin-dev-2026" });

        // ---- onboard with the custom rate, approve
        var create = await admin.PostAsJsonAsync("/api/v1/admin/partners", new
        {
            legalName = "Commission Partner " + tag, bankName = "KBank", bankAccount = "123-4-56789-0", commissionRate = rate,
            owner = new { email, displayName = "Owner " + tag, password = OwnerPw },
            venue = new { slug = "c-" + tag, nameTh = "สนามทดสอบ " + tag, nameEn = "Commission Venue " + tag, district = "Test", address = "1 Test Rd", lat = Lat, lng = Lng },
        });
        var partner = await JsonAsync(create, HttpStatusCode.Created);
        var partnerId = partner.GetProperty("id").GetGuid();
        _partners.Add(partnerId);
        Assert.Equal((decimal)rate, partner.GetProperty("commissionRate").GetDecimal());
        var venueId = partner.GetProperty("venues")[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/v1/admin/partners/{partnerId}/approve", null)).StatusCode);

        // ---- owner sets up 1 court, hours, 300/h price rule through ClubOS
        var owner = await LoginAsync("/api/v1/auth/staff/login", new { email, password = OwnerPw });
        string P(string tail) => $"/api/v1/clubos/venues/{venueId}/{tail}";
        var court = await owner.PostAsJsonAsync(P("courts"), new { name = "Court 1", surface = "hard", indoor = false, airCon = false, floodlit = true, sortOrder = 1 });
        var courtId = (await JsonAsync(court, HttpStatusCode.Created)).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync(P("hours"),
            new { days = Enumerable.Range(0, 7).Select(d => new { dayOfWeek = d, open = "06:00", close = "22:00", slotMinutes = 60 }).ToArray() })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await owner.PostAsJsonAsync(P("prices"),
            new { courtId = (Guid?)null, dayType = "any", start = "06:00", end = "22:00", pricePerHour = 300, label = "Std", priority = 0 })).StatusCode);

        // ---- player holds and pays
        var take0 = (await JsonAsync(await admin.GetAsync(Overview))).GetProperty("takeRevenue").GetDecimal();
        var day = VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime).AddDays(3);
        var player = await api.LoginAsync();
        var held = await player.Http.PostAsJsonAsync("/api/v1/bookings",
            new { venueId, date = day.ToString("yyyy-MM-dd"), slots = new[] { new { courtId, start = "06:00" } } });
        var booking = await JsonAsync(held, HttpStatusCode.Created);
        var id = booking.Id();
        Assert.Equal(300m, booking.GetProperty("total").GetDecimal());
        Assert.True((await player.Http.SimulatePaymentAsync(id)).IsSuccessStatusCode);
        Assert.Equal("confirmed", (await player.Http.GetBookingAsync(id)).Status());

        // ---- commission on the booking row and in the overview take
        decimal commission = 0;
        await api.WithDb(async db => commission = (await db.Database.SqlQuery<decimal>($"SELECT commission AS \"Value\" FROM bookings WHERE id = {id}").ToListAsync()).Single());
        Assert.Equal(expectedCommission, commission);
        Assert.Equal(300m * (decimal)rate, commission);
        var take1 = (await JsonAsync(await admin.GetAsync(Overview))).GetProperty("takeRevenue").GetDecimal();
        Assert.Equal(commission, take1 - take0);

        // ---- play it (in progress now), settle to completed, then move it before the latest Friday cutoff
        var now = api.Clock.GetUtcNow().UtcDateTime;
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE booking_slots SET start_at = {now.AddMinutes(-10)}, end_at = {now.AddMinutes(50)} WHERE booking_id = {id}"));
        Assert.Equal(HttpStatusCode.OK, (await owner.PostAsync(P($"bookings/{id}/check-in"), null)).StatusCode);
        api.Clock.Advance(TimeSpan.FromMinutes(61));
        await api.WithScope(sp => sp.GetRequiredService<BookingService>().SettleBookingsAsync(CancellationToken.None));
        await api.WithDb(async db => Assert.Equal("completed", (await db.Database.SqlQuery<string>($"SELECT status AS \"Value\" FROM bookings WHERE id = {id}").ToListAsync()).Single()));
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE booking_slots SET start_at = start_at - interval '10 days', end_at = end_at - interval '10 days' WHERE booking_id = {id}"));

        var cutoff = PayoutService.LatestCutoff(VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime));
        await api.WithScope(sp => sp.GetRequiredService<PayoutService>().RunAsync(cutoff, CancellationToken.None));

        Api.Domain.Payout? payout = null;
        var periodStart = cutoff.AddDays(-7);
        await api.WithDb(async db => payout = await db.Payouts.AsNoTracking().SingleOrDefaultAsync(x => x.PartnerId == partnerId && x.PeriodStart == periodStart));
        Assert.NotNull(payout);
        Assert.Equal(300m, payout!.Gross);
        Assert.Equal(commission, payout.Commission);
        Assert.Equal(300m * (1m - (decimal)rate), payout.Net);
        Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));
    }
}
