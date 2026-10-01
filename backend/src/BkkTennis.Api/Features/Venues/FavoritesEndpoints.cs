using System.Security.Claims;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Venues;

/// <summary>A player's favourite venues. Adding/removing is idempotent; venues that stop being public simply drop out of the list.</summary>
public static class FavoritesEndpoints
{
    public static RouteGroupBuilder MapFavorites(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/favorites").WithTags("Favorites").RequireAuthorization();

        // Newest favourite first. No availability/distance here (that is the search endpoint's job).
        g.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var userId = user.UserId();
            var favs = await db.Favorites.AsNoTracking().Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt).ThenBy(f => f.VenueId).ToListAsync(ct);
            var ids = favs.Select(f => f.VenueId).ToList();
            var venues = await db.Venues.AsNoTracking()
                .Include(v => v.Courts.Where(c => c.IsActive))
                .Include(v => v.PriceRules)
                .Where(v => ids.Contains(v.Id) && v.Status == "active" && v.Partner!.Status == "active")
                .ToDictionaryAsync(v => v.Id, ct);
            return Results.Ok(favs.Where(f => venues.ContainsKey(f.VenueId)).Select(f => VenueEndpoints.ToSummary(venues[f.VenueId], null, [])));
        });

        g.MapPut("/{venueId:guid}", async (Guid venueId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Venues.AnyAsync(v => v.Id == venueId && v.Status == "active" && v.Partner!.Status == "active", ct))
                return Results.NotFound();
            var userId = user.UserId();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO favorites (user_id, venue_id) VALUES ({userId}, {venueId}) ON CONFLICT DO NOTHING", ct);
            return Results.NoContent();
        });

        g.MapDelete("/{venueId:guid}", async (Guid venueId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var userId = user.UserId();
            await db.Favorites.Where(f => f.UserId == userId && f.VenueId == venueId).ExecuteDeleteAsync(ct);
            return Results.NoContent();
        });

        return api;
    }
}
