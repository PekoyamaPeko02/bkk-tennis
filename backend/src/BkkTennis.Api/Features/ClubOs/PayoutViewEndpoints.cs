using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Admin;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.ClubOs;

/// <summary>
/// Read-only payouts for venue owners. Payouts belong to the partner, not to one venue: the venue in the route only proves
/// ownership, and the list covers every venue of that partner. A payout of another partner is a 404.
/// </summary>
public static class PayoutViewEndpoints
{
    public static void MapPayoutView(this RouteGroupBuilder g)
    {
        g.MapGet("/payouts", async (Guid venueId, int? page, int? pageSize, AppDbContext db, CancellationToken ct) =>
        {
            var partnerId = await PartnerOfAsync(db, venueId, ct);
            var q = db.Payouts.AsNoTracking().Where(p => p.PartnerId == partnerId);
            var size = Math.Clamp(pageSize ?? 25, 1, 100);
            var pageNo = Math.Max(page ?? 1, 1);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(p => p.PeriodStart).ThenByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
                .Skip((pageNo - 1) * size).Take(size).ToListAsync(ct);
            return Results.Ok(new PayoutPageDto(total, pageNo, size, await PayoutEndpoints.ToDtosAsync(db, rows, ct)));
        }).RequireAuthorization(VenueAuthorization.Owner);

        g.MapGet("/payouts/{id:guid}", async (Guid venueId, Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var partnerId = await PartnerOfAsync(db, venueId, ct);
            var payout = await db.Payouts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.PartnerId == partnerId, ct);
            return payout is null ? Results.NotFound() : Results.Ok(await PayoutEndpoints.DetailAsync(db, payout, ct));
        }).RequireAuthorization(VenueAuthorization.Owner);
    }

    private static Task<Guid> PartnerOfAsync(AppDbContext db, Guid venueId, CancellationToken ct) =>
        db.Venues.AsNoTracking().Where(v => v.Id == venueId).Select(v => v.PartnerId).FirstAsync(ct);
}
