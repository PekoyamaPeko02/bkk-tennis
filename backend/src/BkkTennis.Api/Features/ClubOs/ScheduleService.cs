using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.ClubOs;

/// <summary>
/// Cell status: available | held | booked | checked_in | completed | no_show | blocked | past | closed.
/// A cell shows its booking even in the past ("past" only describes empty cells).
/// </summary>
public record ScheduleSlotDto(
    string Start, string End, DateTime StartAt, DateTime EndAt, decimal? Price, string? Label, string Status, Guid? BookingId, Guid? BlockId);
public record ScheduleCourtDto(
    Guid CourtId, string Name, string Surface, bool Indoor, bool AirCon, bool Floodlit, List<ScheduleSlotDto> Slots);
public record ScheduleBookingSlotDto(Guid CourtId, string CourtName, string Start, string End, decimal Price);
/// <summary>PaymentState: paid | pending | failed | refunded | counter (walk-in, paid at the desk) | none.</summary>
public record ScheduleBookingDto(
    Guid Id, string Code, string Status, string Source, string? CustomerName, string? CustomerPhone, string PaymentState,
    decimal Total, decimal AddonTotal, DateTime? HoldExpiresAt, DateTime CreatedAt,
    List<ScheduleBookingSlotDto> Slots, List<BookingAddonDto> Addons);
public record ScheduleBlockDto(Guid Id, Guid CourtId, string Reason, string? Note, string Start, string End, DateTime StartAt, DateTime EndAt);
public record ScheduleKpisDto(
    int SellableSlots, int OccupiedSlots, int HeldSlots, int OccupancyPct,
    decimal Revenue, decimal OnlineRevenue, decimal WalkInRevenue,
    int OnlineBookings, int WalkInBookings, int PendingBookings);
public record ScheduleDto(
    Guid VenueId, string Date, int SlotMinutes, List<ScheduleCourtDto> Courts,
    List<ScheduleBookingDto> Bookings, List<ScheduleBlockDto> Blocks, ScheduleKpisDto Kpis);

/// <summary>The partner portal's day view: the court × time grid with who booked what, blocks, and the day's KPIs.</summary>
public class ScheduleService(AppDbContext db, TimeProvider clock)
{
    private static readonly string[] Live = [BookingStatus.Held, BookingStatus.Confirmed, BookingStatus.CheckedIn];
    private static readonly string[] Finished = [BookingStatus.Completed, BookingStatus.NoShow];
    private static readonly string[] Shown = [.. Live, .. Finished];
    /// <summary>Booking statuses that count as sold (paid or kept): occupancy and revenue.</summary>
    private static readonly string[] Sold = [BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed, BookingStatus.NoShow];

    public async Task<ScheduleDto?> BuildAsync(Guid venueId, DateOnly? requestedDate, CancellationToken ct)
    {
        var venue = await db.Venues.AsNoTracking()
            .Include(v => v.Courts.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
            .Include(v => v.OperatingHours)
            .Include(v => v.PriceRules)
            .FirstOrDefaultAsync(v => v.Id == venueId, ct);
        if (venue is null) return null;

        var tz = venue.Timezone;
        var now = clock.GetUtcNow().UtcDateTime;
        var date = requestedDate ?? VenueClock.Today(tz, now);
        var dayStart = VenueClock.ToUtc(date, TimeOnly.MinValue, tz);
        var dayEnd = VenueClock.ToUtc(date.AddDays(1), TimeOnly.MinValue, tz);
        var courtIds = venue.Courts.Select(c => c.Id).ToList();

        // ---- what happens on this local day
        var bookings = await db.Bookings.AsNoTracking().AsSplitQuery()
            .Include(b => b.Slots).Include(b => b.Payments)
            .Include(b => b.Addons).ThenInclude(a => a.Addon)
            .Where(b => b.VenueId == venueId
                        && b.Slots.Any(s => s.StartAt >= dayStart && s.StartAt < dayEnd && Shown.Contains(s.Status)))
            .ToListAsync(ct);
        var userIds = bookings.Where(b => b.UserId != null).Select(b => b.UserId!.Value).Distinct().ToList();
        var users = await db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, ct);
        var blocks = await db.CourtBlocks.AsNoTracking()
            .Where(b => courtIds.Contains(b.CourtId) && b.StartAt < dayEnd && b.EndAt > dayStart)
            .OrderBy(b => b.StartAt).ThenBy(b => b.CourtId)
            .ToListAsync(ct);

        var daySlots = bookings
            .SelectMany(b => b.Slots.Where(s => s.StartAt >= dayStart && s.StartAt < dayEnd && Shown.Contains(s.Status)))
            .ToList();

        // ---- the grid
        var hours = venue.OperatingHours.FirstOrDefault(h => h.DayOfWeek == (short)date.DayOfWeek);
        var courts = new List<ScheduleCourtDto>();
        foreach (var court in venue.Courts)
        {
            var cells = new List<ScheduleSlotDto>();
            for (var t = hours?.OpenTime ?? TimeOnly.MaxValue; hours is not null && t < hours.CloseTime; t = t.AddMinutes(hours.SlotMinutes))
            {
                var end = t.AddMinutes(hours.SlotMinutes);
                if (end <= t || end > hours.CloseTime) break; // guard against wrap past midnight
                var startUtc = VenueClock.ToUtc(date, t, tz);
                var endUtc = startUtc.AddMinutes(hours.SlotMinutes);
                var price = Pricing.PriceFor(venue.PriceRules, court.Id, date, t, hours.SlotMinutes);

                bool Overlaps(BookingSlot s) => s.CourtId == court.Id && s.StartAt < endUtc && s.EndAt > startUtc;
                var live = daySlots.FirstOrDefault(s => Live.Contains(s.Status) && Overlaps(s));
                var block = blocks.FirstOrDefault(b => b.CourtId == court.Id && b.StartAt < endUtc && b.EndAt > startUtc);
                var finished = daySlots.FirstOrDefault(s => Finished.Contains(s.Status) && Overlaps(s));
                var booked = live ?? finished;

                var status =
                    live is not null ? live.Status switch
                    {
                        BookingStatus.Held => "held",
                        BookingStatus.CheckedIn => "checked_in",
                        _ => "booked",
                    }
                    : block is not null ? "blocked"
                    : finished is not null ? finished.Status // completed | no_show
                    : startUtc <= now ? "past"
                    : price is null ? "closed"
                    : "available";

                cells.Add(new ScheduleSlotDto(t.ToString("HH:mm"), end.ToString("HH:mm"), startUtc, endUtc, price?.Price, price?.Label, status,
                    booked?.BookingId, live is null && block is not null ? block.Id : null));
            }
            courts.Add(new ScheduleCourtDto(court.Id, court.Name, court.Surface, court.Indoor, court.AirCon, court.Floodlit, cells));
        }

        // Names of ALL courts (also deactivated ones): their past bookings are still listed even though they have no grid column.
        var courtNames = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
        var bookingDtos = bookings
            .OrderBy(b => b.Slots.Min(s => s.StartAt)).ThenBy(b => b.Code)
            .Select(b => ToDto(b, users, courtNames, tz))
            .ToList();

        return new ScheduleDto(venue.Id, date.ToString("yyyy-MM-dd"), hours?.SlotMinutes ?? 60, courts, bookingDtos,
            blocks.Select(b => new ScheduleBlockDto(b.Id, b.CourtId, b.Reason, b.Note,
                VenueClock.ToLocal(b.StartAt, tz).ToString("HH:mm"), VenueClock.ToLocal(b.EndAt, tz).ToString("HH:mm"), b.StartAt, b.EndAt)).ToList(),
            Kpis(courts, bookings));
    }

    private static ScheduleKpisDto Kpis(List<ScheduleCourtDto> courts, List<Booking> bookings)
    {
        var cells = courts.SelectMany(c => c.Slots).ToList();
        // capacity = every cell that could be sold: priced (or already sold) and not taken out by a block
        var sellable = cells.Count(c => c.Status != "blocked" && (c.Price is not null || c.BookingId is not null));
        var occupied = cells.Count(c => c.Status is "booked" or "checked_in" or "completed" or "no_show");
        var held = cells.Count(c => c.Status == "held");

        var sold = bookings.Where(b => Sold.Contains(b.Status)).ToList();
        return new ScheduleKpisDto(
            sellable, occupied, held,
            sellable == 0 ? 0 : (int)Math.Round(100.0 * occupied / sellable, MidpointRounding.AwayFromZero),
            sold.Sum(b => b.Total),
            sold.Where(b => b.Source == "app").Sum(b => b.Total),
            sold.Where(b => b.Source == "walk_in").Sum(b => b.Total),
            sold.Count(b => b.Source == "app"),
            sold.Count(b => b.Source == "walk_in"),
            bookings.Count(b => b.Status == BookingStatus.Held));
    }

    private static ScheduleBookingDto ToDto(Booking b, Dictionary<Guid, User> users, Dictionary<Guid, string> courtNames, string tz)
    {
        var user = b.UserId is { } uid && users.TryGetValue(uid, out var u) ? u : null;
        return new ScheduleBookingDto(
            b.Id, b.Code, b.Status, b.Source,
            b.CustomerName ?? user?.DisplayName, PhoneMask.Mask(user?.Phone), PaymentState(b),
            b.Total, b.AddonTotal, b.HoldExpiresAt, b.CreatedAt,
            b.Slots.Where(s => Shown.Contains(s.Status)).OrderBy(s => s.StartAt).ThenBy(s => s.CourtId)
                .Select(s => new ScheduleBookingSlotDto(s.CourtId, courtNames.GetValueOrDefault(s.CourtId, ""),
                    VenueClock.ToLocal(s.StartAt, tz).ToString("HH:mm"), VenueClock.ToLocal(s.EndAt, tz).ToString("HH:mm"), s.Price)).ToList(),
            b.Addons.Select(a => new BookingAddonDto(a.AddonId, a.Addon?.Name ?? "", a.Qty, a.LineTotal)).ToList());
    }

    private static string PaymentState(Booking b)
    {
        if (b.Source == "walk_in") return "counter"; // paid at the desk, no payment row
        var latest = b.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        return latest?.Status switch
        {
            null => "none",
            "succeeded" => "paid",
            var other => other, // pending | failed | refunded
        };
    }
}
