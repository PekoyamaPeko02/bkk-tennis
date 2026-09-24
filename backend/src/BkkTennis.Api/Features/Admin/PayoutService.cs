using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BkkTennis.Api.Features.Admin;

public record PayoutRunResult(DateOnly Cutoff, int Created, int Updated, int Skipped);

/// <summary>
/// Weekly partner payouts. A run with cutoff C (a Friday, Bangkok) settles every <c>completed</c> or <c>no_show</c> online booking (a no-show is still revenue: no refund, partner paid minus commission) whose last slot
/// ended before C 00:00 and that is not in a payout yet, into one payout per partner for the week C-7 … C-1.
/// Money = booking totals the platform collected online; walk-ins are paid at the counter and never appear here.
/// A booking belongs to at most one payout (<c>bookings.payout_id</c>), so re-running is safe and late completions roll forward.
/// </summary>
public class PayoutService(AppDbContext db, TimeProvider clock, ILogger<PayoutService> log)
{
    public const string Timezone = "Asia/Bangkok";

    public static bool IsPayoutDay(DateOnly d) => d.DayOfWeek == DayOfWeek.Friday;

    /// <summary>The most recent Friday on or before <paramref name="today"/>.</summary>
    public static DateOnly LatestCutoff(DateOnly today) => today.AddDays(-(((int)today.DayOfWeek - (int)DayOfWeek.Friday + 7) % 7));

    public DateOnly Today => VenueClock.Today(Timezone, clock.GetUtcNow().UtcDateTime);

    private IQueryable<Booking> Eligible(DateTime cutoffUtc) =>
        db.Bookings.Where(b => (b.Status == BookingStatus.Completed || b.Status == BookingStatus.NoShow) && b.Source == "app" && b.PayoutId == null
                               && b.Slots.Any() && b.Slots.Max(s => s.EndAt) <= cutoffUtc);

    public async Task<PayoutRunResult> RunAsync(DateOnly cutoff, CancellationToken ct)
    {
        var cutoffUtc = VenueClock.ToUtc(cutoff, TimeOnly.MinValue, Timezone);
        var partnerIds = await Eligible(cutoffUtc).Select(b => b.Venue!.PartnerId).Distinct().ToListAsync(ct);

        int created = 0, updated = 0, skipped = 0;
        foreach (var pid in partnerIds)
        {
            var outcome = await SettleAsync(pid, cutoff, cutoffUtc, ct);
            db.ChangeTracker.Clear();
            switch (outcome)
            {
                case Outcome.Created: created++; break;
                case Outcome.Updated: updated++; break;
                case Outcome.Skipped: skipped++; break;
            }
        }
        if (created + updated + skipped > 0) log.LogInformation("Payout run {Cutoff}: {Created} created, {Updated} updated, {Skipped} skipped", cutoff, created, updated, skipped);
        return new PayoutRunResult(cutoff, created, updated, skipped);
    }

    private enum Outcome { Nothing, Created, Updated, Skipped }

    private async Task<Outcome> SettleAsync(Guid partnerId, DateOnly cutoff, DateTime cutoffUtc, CancellationToken ct)
    {
        var start = cutoff.AddDays(-7);
        var end = cutoff.AddDays(-1);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Lock the week's payout (if any) so two runners can't overwrite each other's totals.
        var payout = (await db.Payouts.FromSqlInterpolated(
            $"SELECT * FROM payouts WHERE partner_id = {partnerId} AND period_start = {start} AND period_end = {end} FOR UPDATE")
            .ToListAsync(ct)).FirstOrDefault();
        // Already paid (or failed and awaiting a human): these bookings wait for next week's payout instead.
        if (payout is { Status: not PayoutStatus.Pending }) return Outcome.Skipped;

        var isNew = payout is null;
        if (payout is null)
        {
            payout = new Payout { Id = Guid.NewGuid(), PartnerId = partnerId, PeriodStart = start, PeriodEnd = end, CreatedAt = clock.GetUtcNow().UtcDateTime };
            db.Payouts.Add(payout);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Outcome.Skipped; // another runner created it a moment ago and covers these bookings
            }
        }

        var now = clock.GetUtcNow().UtcDateTime;
        var payoutId = payout.Id;
        var claimed = await Eligible(cutoffUtc)
            .Where(b => db.Venues.Any(v => v.Id == b.VenueId && v.PartnerId == partnerId))
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.PayoutId, payoutId).SetProperty(b => b.UpdatedAt, now), ct);
        if (claimed == 0)
        {
            await tx.RollbackAsync(ct);
            return Outcome.Nothing;
        }

        var sums = await db.Bookings.Where(b => b.PayoutId == payoutId)
            .GroupBy(_ => 1).Select(g => new { Gross = g.Sum(b => b.Total), Commission = g.Sum(b => b.Commission) }).FirstAsync(ct);
        payout.Gross = sums.Gross;
        payout.Commission = sums.Commission;
        payout.Net = sums.Gross - sums.Commission;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return isNew ? Outcome.Created : Outcome.Updated;
    }
}
