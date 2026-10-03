using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Admin;

public record OverviewDayDto(DateOnly Date, int Bookings, decimal Gbv, decimal Take);
public record AdminOverviewDto(
    decimal Gbv, decimal TakeRevenue, int Bookings, int WalkInBookings,
    int ActivePartners, int ActiveVenues, int ActiveCourts, List<OverviewDayDto> Last30Days);

/// <summary>Platform-admin endpoints (Admin portal). Every route requires the <c>platform_admin</c> role.</summary>
public static class AdminEndpoints
{
    /// <summary>Bookings whose money the platform still holds: paid and not refunded (a no-show keeps its money, no refund).</summary>
    private static readonly string[] Sold = [BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed, BookingStatus.NoShow];
    private const string PlatformTimezone = "Asia/Bangkok";
    private const int SeriesDays = 30;

    public static RouteGroupBuilder MapAdmin(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/admin")
            .RequireAuthorization(p => p.RequireAuthenticatedUser().RequireRole(Roles.PlatformAdmin))
            .WithTags("Admin");

        // GBV / take are app (online) sales only: walk-ins are paid at the counter and carry 0 commission, so they are counted apart.
        g.MapGet("/overview", async (AppDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            var online = db.Bookings.AsNoTracking().Where(b => Sold.Contains(b.Status) && b.Source == "app");
            var gbv = await online.SumAsync(b => (decimal?)b.Total, ct) ?? 0m;
            var take = await online.SumAsync(b => (decimal?)b.Commission, ct) ?? 0m;
            var bookings = await online.CountAsync(ct);
            var walkIns = await db.Bookings.AsNoTracking().CountAsync(b => Sold.Contains(b.Status) && b.Source == "walk_in", ct);

            var activePartners = await db.Partners.CountAsync(p => p.Status == "active", ct);
            var activeVenues = await db.Venues.CountAsync(v => v.Status == "active" && v.Partner!.Status == "active", ct);
            var activeCourts = await db.Courts.CountAsync(c => c.IsActive && db.Venues.Any(v => v.Id == c.VenueId && v.Status == "active" && v.Partner!.Status == "active"), ct);

            var now = clock.GetUtcNow().UtcDateTime;
            var today = VenueClock.Today(PlatformTimezone, now);
            var first = today.AddDays(-(SeriesDays - 1));
            var from = VenueClock.ToUtc(first, TimeOnly.MinValue, PlatformTimezone);
            var rows = await online.Where(b => b.CreatedAt >= from).Select(b => new { b.CreatedAt, b.Total, b.Commission }).ToListAsync(ct);
            var byDay = rows
                .GroupBy(r => DateOnly.FromDateTime(VenueClock.ToLocal(r.CreatedAt, PlatformTimezone)))
                .ToDictionary(x => x.Key, x => (n: x.Count(), gbv: x.Sum(r => r.Total), take: x.Sum(r => r.Commission)));
            var series = Enumerable.Range(0, SeriesDays).Select(i =>
            {
                var d = first.AddDays(i);
                return byDay.TryGetValue(d, out var v) ? new OverviewDayDto(d, v.n, v.gbv, v.take) : new OverviewDayDto(d, 0, 0, 0);
            }).ToList();

            return Results.Ok(new AdminOverviewDto(gbv, take, bookings, walkIns, activePartners, activeVenues, activeCourts, series));
        });

        g.MapPartners();
        g.MapBookingExplorer();
        g.MapPayouts();

        return api;
    }
}
