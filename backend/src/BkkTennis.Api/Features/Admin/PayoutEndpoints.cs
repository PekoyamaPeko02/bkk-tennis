using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Admin;

/// <summary>Period dates are yyyy-MM-dd (Bangkok); Bank* come from the partner so finance knows where to transfer.</summary>
public record PayoutDto(
    Guid Id, Guid PartnerId, string PartnerName, string? BankName, string? BankAccount, string PeriodStart, string PeriodEnd, int BookingCount,
    decimal Gross, decimal Commission, decimal Net, string Status, string? TransferRef, DateTime? PaidAt, DateTime CreatedAt);
public record PayoutBookingDto(Guid Id, string Code, string VenueName, string? Date, decimal Total, decimal Commission);
public record PayoutDetailDto(PayoutDto Payout, List<PayoutBookingDto> Bookings);
public record PayoutPageDto(int Total, int Page, int PageSize, List<PayoutDto> Items);
public record MarkPaidRequest([property: Required, MaxLength(80)] string TransferRef);

public static class PayoutEndpoints
{
    private static readonly string[] Statuses = [PayoutStatus.Pending, PayoutStatus.Paid, PayoutStatus.Failed];

    public static void MapPayouts(this RouteGroupBuilder g)
    {
        g.MapGet("/payouts", async (string? status, Guid? partnerId, int? page, int? pageSize, AppDbContext db, CancellationToken ct) =>
        {
            if (status is not null && !Statuses.Contains(status))
                throw new BookingException(400, "bad_status", "status ต้องเป็น pending, paid หรือ failed");
            var q = db.Payouts.AsNoTracking().AsQueryable();
            if (status is not null) q = q.Where(p => p.Status == status);
            if (partnerId is { } pid) q = q.Where(p => p.PartnerId == pid);
            var size = Math.Clamp(pageSize ?? 25, 1, 100);
            var pageNo = Math.Max(page ?? 1, 1);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(p => p.PeriodStart).ThenByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
                .Skip((pageNo - 1) * size).Take(size).ToListAsync(ct);
            return Results.Ok(new PayoutPageDto(total, pageNo, size, await ToDtosAsync(db, rows, ct)));
        });

        g.MapGet("/payouts/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var payout = await db.Payouts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (payout is null) return Results.NotFound();
            var bookings = await db.Bookings.AsNoTracking().Include(b => b.Venue).Include(b => b.Slots)
                .Where(b => b.PayoutId == id).OrderBy(b => b.Code).ToListAsync(ct);
            var items = bookings.Select(b =>
            {
                var first = b.Slots.OrderBy(s => s.StartAt).FirstOrDefault();
                var date = first is null ? null
                    : DateOnly.FromDateTime(VenueClock.ToLocal(first.StartAt, b.Venue?.Timezone ?? PayoutService.Timezone)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return new PayoutBookingDto(b.Id, b.Code, b.Venue?.NameEn ?? "", date, b.Total, b.Commission);
            }).ToList();
            return Results.Ok(new PayoutDetailDto((await ToDtosAsync(db, [payout], ct))[0], items));
        });

        // Runs the weekly settlement by hand (the Friday job does the same). cutoff must be a Friday that is not in the future.
        g.MapPost("/payouts/run", async (DateOnly? cutoff, PayoutService payouts, CancellationToken ct) =>
        {
            var today = payouts.Today;
            var c = cutoff ?? PayoutService.LatestCutoff(today);
            if (!PayoutService.IsPayoutDay(c)) throw new BookingException(400, "not_friday", "รอบจ่ายเงินต้องเป็นวันศุกร์");
            if (c > today) throw new BookingException(400, "future_cutoff", "ยังไม่ถึงรอบจ่ายเงินนี้");
            return Results.Ok(await payouts.RunAsync(c, ct));
        });

        // pending|failed → paid. Atomic, so a double click can't overwrite the first transfer reference.
        g.MapPost("/payouts/{id:guid}/mark-paid", async (Guid id, MarkPaidRequest req, AppDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            var reference = req.TransferRef.Trim();
            if (reference.Length == 0) throw new BookingException(400, "bad_transfer_ref", "กรุณาระบุเลขอ้างอิงการโอน");
            var now = clock.GetUtcNow().UtcDateTime;
            var n = await db.Payouts.Where(p => p.Id == id && p.Status != PayoutStatus.Paid)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PayoutStatus.Paid).SetProperty(p => p.TransferRef, reference).SetProperty(p => p.PaidAt, now), ct);
            if (n == 0)
            {
                if (!await db.Payouts.AnyAsync(p => p.Id == id, ct)) return Results.NotFound();
                throw new BookingException(409, "bad_transition", "Payout นี้จ่ายแล้ว");
            }
            var payout = await db.Payouts.AsNoTracking().FirstAsync(p => p.Id == id, ct);
            return Results.Ok((await ToDtosAsync(db, [payout], ct))[0]);
        }).WithValidation();
    }

    private static async Task<List<PayoutDto>> ToDtosAsync(AppDbContext db, List<Payout> payouts, CancellationToken ct)
    {
        var ids = payouts.Select(p => p.Id).ToList();
        var partnerIds = payouts.Select(p => p.PartnerId).Distinct().ToList();
        var partners = await db.Partners.AsNoTracking().Where(p => partnerIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        var counts = await db.Bookings.AsNoTracking().Where(b => b.PayoutId != null && ids.Contains(b.PayoutId.Value))
            .GroupBy(b => b.PayoutId!.Value).Select(g => new { Id = g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.N, ct);
        return payouts.Select(p =>
        {
            partners.TryGetValue(p.PartnerId, out var partner);
            return new PayoutDto(
                p.Id, p.PartnerId, partner?.LegalName ?? "", partner?.BankName, partner?.BankAccount,
                p.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), p.PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                counts.GetValueOrDefault(p.Id), p.Gross, p.Commission, p.Net, p.Status, p.TransferRef, p.PaidAt, p.CreatedAt);
        }).ToList();
    }
}
