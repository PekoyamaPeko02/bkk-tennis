using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Services;

public record SlotDto(string Start, string End, DateTime StartAt, DateTime EndAt, decimal? Price, string? Label, string Status);

public record CourtAvailabilityDto(
    Guid CourtId, string Name, string Surface, bool Indoor, bool AirCon, bool Floodlit, List<SlotDto> Slots);

public record VenueAvailabilityDto(Guid VenueId, DateOnly Date, int SlotMinutes, List<CourtAvailabilityDto> Courts);

/// <summary>
/// Slot status: available | booked | held | blocked | past | closed.
/// "held" is shown separately so the UI can say "someone is paying for this".
/// </summary>
public class AvailabilityService(AppDbContext db, TimeProvider clock)
{
    public async Task<VenueAvailabilityDto?> ForVenueAsync(Guid venueId, DateOnly date, CancellationToken ct = default)
    {
        var venue = await db.Venues.AsNoTracking()
            .Include(v => v.Courts.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
            .Include(v => v.OperatingHours)
            .Include(v => v.PriceRules)
            .FirstOrDefaultAsync(v => v.Id == venueId && v.Status == "active" && v.Partner!.Status == "active", ct);
        if (venue is null) return null;

        return await BuildAsync(venue, date, ct);
    }

    public async Task<VenueAvailabilityDto> BuildAsync(Venue venue, DateOnly date, CancellationToken ct = default)
    {
        var hours = venue.OperatingHours.FirstOrDefault(h => h.DayOfWeek == (short)date.DayOfWeek);
        if (hours is null)
            return new VenueAvailabilityDto(venue.Id, date, 60,
                venue.Courts.Select(c => ToCourt(c, [])).ToList());

        var dayStart = VenueClock.ToUtc(date, hours.OpenTime, venue.Timezone);
        var dayEnd = VenueClock.ToUtc(date, hours.CloseTime, venue.Timezone);
        var courtIds = venue.Courts.Select(c => c.Id).ToList();

        var taken = await db.BookingSlots.AsNoTracking()
            .Where(s => courtIds.Contains(s.CourtId)
                        && BookingStatus.Active.Contains(s.Status)
                        && s.StartAt < dayEnd && s.EndAt > dayStart)
            .Select(s => new { s.CourtId, s.StartAt, s.EndAt, s.Status })
            .ToListAsync(ct);

        var blocks = await db.CourtBlocks.AsNoTracking()
            .Where(b => courtIds.Contains(b.CourtId) && b.StartAt < dayEnd && b.EndAt > dayStart)
            .Select(b => new { b.CourtId, b.StartAt, b.EndAt })
            .ToListAsync(ct);

        var now = clock.GetUtcNow().UtcDateTime;
        var courts = new List<CourtAvailabilityDto>();
        foreach (var court in venue.Courts)
        {
            var slots = new List<SlotDto>();
            for (var t = hours.OpenTime; t < hours.CloseTime; t = t.AddMinutes(hours.SlotMinutes))
            {
                var end = t.AddMinutes(hours.SlotMinutes);
                if (end <= t || end > hours.CloseTime) break; // guard against wrap past midnight
                var startUtc = VenueClock.ToUtc(date, t, venue.Timezone);
                var endUtc = startUtc.AddMinutes(hours.SlotMinutes);
                var price = Pricing.PriceFor(venue.PriceRules, court.Id, date, t, hours.SlotMinutes);

                var hit = taken.FirstOrDefault(s => s.CourtId == court.Id && s.StartAt < endUtc && s.EndAt > startUtc);
                var status =
                    startUtc <= now ? "past"
                    : blocks.Any(b => b.CourtId == court.Id && b.StartAt < endUtc && b.EndAt > startUtc) ? "blocked"
                    : hit is not null ? (hit.Status == BookingStatus.Held ? "held" : "booked")
                    : price is null ? "closed"
                    : "available";

                slots.Add(new SlotDto(t.ToString("HH:mm"), end.ToString("HH:mm"), startUtc, endUtc,
                    price?.Price, price?.Label, status));
            }
            courts.Add(ToCourt(court, slots));
        }

        return new VenueAvailabilityDto(venue.Id, date, hours.SlotMinutes, courts);
    }

    private static CourtAvailabilityDto ToCourt(Court c, List<SlotDto> slots) =>
        new(c.Id, c.Name, c.Surface, c.Indoor, c.AirCon, c.Floodlit, slots);
}
