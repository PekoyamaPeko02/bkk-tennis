using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BkkTennis.Api.Features.Venues;

public record VenueSummaryDto(
    Guid Id, string Slug, string NameTh, string NameEn, string District, string Address,
    double Lat, double Lng, double? DistanceKm, string? TransitNote, decimal Rating, int RatingCount,
    string[] Amenities, string[] Surfaces, bool HasIndoor, bool HasAirCon, int CourtCount,
    decimal? PriceFrom, decimal? PriceTo, List<string> NextFreeTimes, int FreeSlotCount, string? CoverImageUrl);

public record AddonDto(Guid Id, string Name, string? Description, string? Icon, decimal Price, string Unit);

public record VenueDetailDto(
    VenueSummaryDto Summary, int CancelFreeHours, List<CourtDto> Courts, List<AddonDto> Addons);

public record CourtDto(Guid Id, string Name, string Surface, bool Indoor, bool AirCon, bool Floodlit);

public static class VenueEndpoints
{
    public static RouteGroupBuilder MapVenues(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/venues").WithTags("Venues");

        // GET /venues?lat=13.73&lng=100.57&radiusKm=5&date=2026-10-24&from=17:00&to=22:00&surface=hard&indoor=true&sort=distance
        g.MapGet("/", async (
            double? lat, double? lng, double? radiusKm, DateOnly? date,
            [FromQuery(Name = "from")] string? fromTime, [FromQuery(Name = "to")] string? toTime,
            string? surface, bool? indoor, string? q, string? sort,
            AppDbContext db, AvailabilityService availability, CancellationToken ct) =>
        {
            // 1) candidate venues + distance from PostGIS
            var distances = await NearbyAsync(db, lat, lng, radiusKm, ct);

            var query = db.Venues.AsNoTracking()
                .Include(v => v.Courts.Where(c => c.IsActive))
                .Include(v => v.OperatingHours)
                .Include(v => v.PriceRules)
                .Where(v => v.Status == "active" && v.Partner!.Status == "active");
            if (distances is not null)
            {
                var ids = distances.Keys.ToList();
                query = query.Where(v => ids.Contains(v.Id));
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                var like = $"%{q.Trim()}%";
                query = query.Where(v => EF.Functions.ILike(v.NameEn, like) || EF.Functions.ILike(v.NameTh, like)
                                         || EF.Functions.ILike(v.District, like));
            }
            var venues = await query.ToListAsync(ct);

            // 2) filter by court features
            if (!string.IsNullOrWhiteSpace(surface))
                venues = venues.Where(v => v.Courts.Any(c => c.Surface == surface)).ToList();
            if (indoor == true)
                venues = venues.Where(v => v.Courts.Any(c => c.Indoor)).ToList();

            // 3) availability for the requested day/time window
            var day = date ?? VenueClock.Today("Asia/Bangkok");
            var fromT = TimeOnly.TryParse(fromTime, out var f) ? f : TimeOnly.MinValue;
            var toT = TimeOnly.TryParse(toTime, out var t) ? t : TimeOnly.MaxValue;

            var result = new List<VenueSummaryDto>();
            foreach (var v in venues)
            {
                var avail = await availability.BuildAsync(v, day, ct);
                var free = avail.Courts.SelectMany(c => c.Slots)
                    .Where(s => s.Status == "available")
                    .Where(s => TimeOnly.Parse(s.Start) >= fromT && TimeOnly.Parse(s.Start) < toT)
                    .ToList();
                double? km = distances is not null && distances.TryGetValue(v.Id, out var m) ? Math.Round(m / 1000, 1) : null;
                result.Add(ToSummary(v, km, free));
            }

            result = (sort ?? "distance") switch
            {
                "price" => result.OrderBy(r => r.PriceFrom ?? decimal.MaxValue).ToList(),
                "rating" => result.OrderByDescending(r => r.Rating).ToList(),
                "earliest" => result.OrderBy(r => r.NextFreeTimes.FirstOrDefault() ?? "99:99").ToList(),
                _ => result.OrderBy(r => r.DistanceKm ?? double.MaxValue).ThenByDescending(r => r.Rating).ToList(),
            };
            return Results.Ok(new { date = day, count = result.Count, items = result });
        });

        g.MapGet("/{idOrSlug}", async (string idOrSlug, double? lat, double? lng, AppDbContext db, CancellationToken ct) =>
        {
            var q = db.Venues.AsNoTracking()
                .Include(v => v.Courts.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
                .Include(v => v.PriceRules)
                .Include(v => v.Addons.Where(a => a.IsActive))
                .Where(v => v.Status == "active" && v.Partner!.Status == "active");
            var v = Guid.TryParse(idOrSlug, out var id)
                ? await q.FirstOrDefaultAsync(x => x.Id == id, ct)
                : await q.FirstOrDefaultAsync(x => x.Slug == idOrSlug, ct);
            if (v is null) return Results.NotFound();

            double? km = lat is not null && lng is not null ? Math.Round(Haversine(lat.Value, lng.Value, v.Lat, v.Lng), 1) : null;
            return Results.Ok(new VenueDetailDto(
                ToSummary(v, km, []),
                v.CancelFreeHours,
                v.Courts.Select(c => new CourtDto(c.Id, c.Name, c.Surface, c.Indoor, c.AirCon, c.Floodlit)).ToList(),
                v.Addons.Select(a => new AddonDto(a.Id, a.Name, a.Description, a.Icon, a.Price, a.Unit)).ToList()));
        });

        g.MapGet("/{id:guid}/availability", async (Guid id, DateOnly? date, AvailabilityService availability, CancellationToken ct) =>
        {
            var result = await availability.ForVenueAsync(id, date ?? VenueClock.Today("Asia/Bangkok"), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        return api;
    }

    private static VenueSummaryDto ToSummary(Venue v, double? km, List<SlotDto> free)
    {
        var prices = v.PriceRules.Select(p => p.PricePerHour).ToList();
        return new VenueSummaryDto(
            v.Id, v.Slug, v.NameTh, v.NameEn, v.District, v.Address, v.Lat, v.Lng, km, v.TransitNote,
            v.RatingAvg, v.RatingCount, v.AmenityList(),
            v.Courts.Select(c => c.Surface).Distinct().ToArray(),
            v.Courts.Any(c => c.Indoor), v.Courts.Any(c => c.AirCon), v.Courts.Count,
            prices.Count > 0 ? prices.Min() : null, prices.Count > 0 ? prices.Max() : null,
            free.Select(s => s.Start).Distinct().OrderBy(s => s).Take(3).ToList(),
            free.Count, v.CoverImageUrl);
    }

    /// <summary>venueId → distance in metres, or null when no location was supplied.</summary>
    private static async Task<Dictionary<Guid, double>?> NearbyAsync(
        AppDbContext db, double? lat, double? lng, double? radiusKm, CancellationToken ct)
    {
        if (lat is null || lng is null) return null;
        var radiusM = Math.Clamp(radiusKm ?? 5, 0.5, 60) * 1000;

        const string sql = """
            SELECT v.id, ST_Distance(v.location, ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography) AS dist
            FROM venues v
            JOIN partners p ON p.id = v.partner_id
            WHERE v.status = 'active' AND p.status = 'active'
              AND ST_DWithin(v.location, ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography, @radius)
            """;

        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("lat", lat.Value);
            cmd.Parameters.AddWithValue("lng", lng.Value);
            cmd.Parameters.AddWithValue("radius", radiusM);
            var map = new Dictionary<Guid, double>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) map[reader.GetGuid(0)] = reader.GetDouble(1);
            return map;
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    internal static double Haversine(double lat1, double lng1, double lat2, double lng2)
    {
        const double r = 6371.0;
        double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Pow(Math.Sin(dLng / 2), 2);
        return 2 * r * Math.Asin(Math.Sqrt(a));
    }
}
