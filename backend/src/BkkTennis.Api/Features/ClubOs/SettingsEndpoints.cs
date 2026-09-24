using System.ComponentModel.DataAnnotations;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.ClubOs;

public record CourtSettingsDto(Guid Id, string Name, string Surface, bool Indoor, bool AirCon, bool Floodlit, int SortOrder, bool IsActive);
public record CourtRequest(
    [property: Required, MaxLength(60)] string Name, [property: Required, MaxLength(20)] string Surface,
    bool Indoor, bool AirCon, bool Floodlit, int SortOrder, bool IsActive = true);
public record HoursDto(int DayOfWeek, string Open, string Close, int SlotMinutes);
public record HoursRequest([property: Required, MaxLength(7)] List<HoursDto> Days);
public record AddonSettingsDto(Guid Id, string Name, string? Description, string? Icon, decimal Price, string Unit, bool IsActive);
public record AddonSettingsRequest(
    [property: Required, MaxLength(80)] string Name, [property: MaxLength(200)] string? Description, [property: MaxLength(40)] string? Icon,
    decimal Price, [property: Required, MaxLength(20)] string Unit, bool IsActive = true);
public record CancelPolicyRequest(int CancelFreeHours);
public record VenueSettingsDto(int CancelFreeHours, List<CourtSettingsDto> Courts, List<HoursDto> Hours, List<AddonSettingsDto> Addons);
public record PricePreviewSlotDto(string Start, string End, decimal? Price, string? Label);
public record PricePreviewCourtDto(Guid CourtId, string Name, List<PricePreviewSlotDto> Slots);
public record PricePreviewDto(DateOnly Date, int SlotMinutes, List<PricePreviewCourtDto> Courts);

/// <summary>ClubOS venue settings: courts, opening hours, add-ons, cancellation policy, price preview. Reads = staff, writes = owner.</summary>
public static class SettingsEndpoints
{
    private static readonly string[] Surfaces = ["hard", "clay", "grass", "synthetic"];
    private static readonly string[] AddonUnits = ["per_booking", "per_hour"];
    private const int HorizonDays = 30;

    public static void MapSettings(this RouteGroupBuilder g)
    {
        g.MapGet("/settings", async (Guid venueId, AppDbContext db, CancellationToken ct) =>
        {
            var venue = await db.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == venueId, ct);
            if (venue is null) return Results.NotFound();
            var courts = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
            var hours = await db.OperatingHours.AsNoTracking().Where(h => h.VenueId == venueId).OrderBy(h => h.DayOfWeek).ToListAsync(ct);
            var addons = await db.Addons.AsNoTracking().Where(a => a.VenueId == venueId).OrderBy(a => a.Name).ToListAsync(ct);
            return Results.Ok(new VenueSettingsDto(venue.CancelFreeHours, courts.Select(ToDto).ToList(),
                hours.Select(ToDto).ToList(), addons.Select(ToDto).ToList()));
        });

        g.MapPut("/settings/cancel-policy", async (Guid venueId, CancelPolicyRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (req.CancelFreeHours is < 0 or > 168)
                throw new BookingException(400, "bad_cancel_hours", "ชั่วโมงยกเลิกฟรีต้องอยู่ระหว่าง 0-168");
            var venue = await db.Venues.FirstOrDefaultAsync(v => v.Id == venueId, ct);
            if (venue is null) return Results.NotFound();
            venue.CancelFreeHours = req.CancelFreeHours;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { cancelFreeHours = venue.CancelFreeHours });
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        // ---- courts (never deleted: bookings reference them — deactivate instead)
        g.MapPost("/courts", async (Guid venueId, CourtRequest req, AppDbContext db, AvailabilityNotifier notifier, TimeProvider clock, CancellationToken ct) =>
        {
            var court = new Court { Id = Guid.NewGuid(), VenueId = venueId };
            Apply(court, req);
            db.Courts.Add(court);
            await db.SaveChangesAsync(ct);
            await NotifyAsync(db, notifier, clock, venueId);
            return Results.Created($"/api/v1/clubos/venues/{venueId}/settings", ToDto(court));
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        g.MapPut("/courts/{courtId:guid}", async (
            Guid venueId, Guid courtId, CourtRequest req, AppDbContext db, AvailabilityNotifier notifier, TimeProvider clock, CancellationToken ct) =>
        {
            var court = await db.Courts.FirstOrDefaultAsync(c => c.Id == courtId && c.VenueId == venueId, ct);
            if (court is null) return Results.NotFound();
            if (court.IsActive && !req.IsActive)
            {
                var now = clock.GetUtcNow().UtcDateTime;
                var live = await db.BookingSlots.AsNoTracking()
                    .Where(s => s.CourtId == courtId && BookingStatus.Active.Contains(s.Status) && s.EndAt > now)
                    .Select(s => s.BookingId).Distinct()
                    .Join(db.Bookings, id => id, b => b.Id, (_, b) => b.Code).ToListAsync(ct);
                if (live.Count > 0)
                    throw new BookingException(409, "court_has_bookings",
                        $"คอร์ทนี้ยังมีการจองที่ยังไม่สิ้นสุด ({string.Join(", ", live.Take(5))}) กรุณายกเลิกก่อนปิดคอร์ท");
            }
            Apply(court, req);
            await db.SaveChangesAsync(ct);
            await NotifyAsync(db, notifier, clock, venueId);
            return Results.Ok(ToDto(court));
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        // ---- opening hours: the request replaces the whole week; a day that is left out is closed.
        // Existing bookings are untouched (they keep their slots even if the day is now closed).
        g.MapPut("/hours", async (Guid venueId, HoursRequest req, AppDbContext db, AvailabilityNotifier notifier, TimeProvider clock, CancellationToken ct) =>
        {
            var rows = new List<OperatingHours>();
            foreach (var d in req.Days)
            {
                if (d.DayOfWeek is < 0 or > 6)
                    throw new BookingException(400, "bad_day", "วันต้องอยู่ระหว่าง 0 (อาทิตย์) - 6 (เสาร์)");
                if (rows.Any(r => r.DayOfWeek == d.DayOfWeek))
                    throw new BookingException(400, "duplicate_day", "มีวันซ้ำกัน");
                if (!TimeOnly.TryParseExact(d.Open, "HH:mm", out var open) || !TimeOnly.TryParseExact(d.Close, "HH:mm", out var close))
                    throw new BookingException(400, "bad_time", "รูปแบบเวลาไม่ถูกต้อง (HH:mm)");
                if (close <= open)
                    throw new BookingException(400, "bad_range", "เวลาปิดต้องหลังเวลาเปิด");
                if (d.SlotMinutes is not (30 or 60))
                    throw new BookingException(400, "bad_slot_minutes", "ช่วงเวลาต่อสล็อตต้องเป็น 30 หรือ 60 นาที");
                if ((close - open).TotalMinutes % d.SlotMinutes != 0)
                    throw new BookingException(400, "bad_range", "ช่วงเวลาเปิด-ปิดต้องหารด้วยความยาวสล็อตลงตัว");
                rows.Add(new OperatingHours { VenueId = venueId, DayOfWeek = (short)d.DayOfWeek, OpenTime = open, CloseTime = close, SlotMinutes = (short)d.SlotMinutes });
            }
            var existing = await db.OperatingHours.Where(h => h.VenueId == venueId).ToListAsync(ct);
            db.OperatingHours.RemoveRange(existing);
            db.OperatingHours.AddRange(rows);
            await db.SaveChangesAsync(ct);
            await NotifyAsync(db, notifier, clock, venueId);
            return Results.Ok(rows.OrderBy(r => r.DayOfWeek).Select(ToDto).ToList());
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        // ---- add-ons (never deleted: booking_addons reference them — deactivate instead)
        g.MapPost("/addons", async (Guid venueId, AddonSettingsRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var addon = new Addon { Id = Guid.NewGuid(), VenueId = venueId };
            Apply(addon, req);
            db.Addons.Add(addon);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/clubos/venues/{venueId}/settings", ToDto(addon));
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        g.MapPut("/addons/{addonId:guid}", async (Guid venueId, Guid addonId, AddonSettingsRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var addon = await db.Addons.FirstOrDefaultAsync(a => a.Id == addonId && a.VenueId == venueId, ct);
            if (addon is null) return Results.NotFound();
            Apply(addon, req);
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDto(addon));
        }).RequireAuthorization(VenueAuthorization.Owner).WithValidation();

        // ---- what a player would be charged on a given day (same code path as booking: Pricing.PriceFor)
        g.MapGet("/prices/preview", async (Guid venueId, DateOnly date, AppDbContext db, CancellationToken ct) =>
        {
            var courts = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(ct);
            var rules = await db.PriceRules.AsNoTracking().Where(r => r.VenueId == venueId).ToListAsync(ct);
            var hours = await db.OperatingHours.AsNoTracking().FirstOrDefaultAsync(h => h.VenueId == venueId && h.DayOfWeek == (short)date.DayOfWeek, ct);
            var minutes = hours?.SlotMinutes ?? 60;
            var result = courts.Select(c =>
            {
                var slots = new List<PricePreviewSlotDto>();
                if (hours is not null)
                    for (var t = hours.OpenTime; t < hours.CloseTime; t = t.AddMinutes(minutes))
                    {
                        var p = Pricing.PriceFor(rules, c.Id, date, t, minutes);
                        slots.Add(new PricePreviewSlotDto(t.ToString("HH:mm"), t.AddMinutes(minutes).ToString("HH:mm"), p?.Price, p?.Label));
                    }
                return new PricePreviewCourtDto(c.Id, c.Name, slots);
            }).ToList();
            return Results.Ok(new PricePreviewDto(date, minutes, result));
        });
    }

    private static CourtSettingsDto ToDto(Court c) => new(c.Id, c.Name, c.Surface, c.Indoor, c.AirCon, c.Floodlit, c.SortOrder, c.IsActive);
    private static HoursDto ToDto(OperatingHours h) => new(h.DayOfWeek, h.OpenTime.ToString("HH:mm"), h.CloseTime.ToString("HH:mm"), h.SlotMinutes);
    private static AddonSettingsDto ToDto(Addon a) => new(a.Id, a.Name, a.Description, a.Icon, a.Price, a.Unit, a.IsActive);

    private static void Apply(Court court, CourtRequest req)
    {
        var name = req.Name.Trim();
        if (name.Length == 0) throw new BookingException(400, "bad_name", "กรุณาระบุชื่อคอร์ท");
        if (!Surfaces.Contains(req.Surface))
            throw new BookingException(400, "bad_surface", "พื้นผิวต้องเป็น hard, clay, grass หรือ synthetic");
        court.Name = name;
        court.Surface = req.Surface;
        court.Indoor = req.Indoor;
        court.AirCon = req.AirCon;
        court.Floodlit = req.Floodlit;
        court.SortOrder = req.SortOrder;
        court.IsActive = req.IsActive;
    }

    private static void Apply(Addon addon, AddonSettingsRequest req)
    {
        var name = req.Name.Trim();
        if (name.Length == 0) throw new BookingException(400, "bad_name", "กรุณาระบุชื่อบริการ");
        if (!AddonUnits.Contains(req.Unit))
            throw new BookingException(400, "bad_unit", "หน่วยต้องเป็น per_booking หรือ per_hour");
        if (req.Price < 0 || req.Price > 100_000)
            throw new BookingException(400, "bad_price", "ราคาไม่ถูกต้อง");
        addon.Name = name;
        addon.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        addon.Icon = string.IsNullOrWhiteSpace(req.Icon) ? null : req.Icon.Trim();
        addon.Price = decimal.Round(req.Price, 2);
        addon.Unit = req.Unit;
        addon.IsActive = req.IsActive;
    }

    /// <summary>Courts/hours changed: tell open availability views to refetch the next month.</summary>
    private static async Task NotifyAsync(AppDbContext db, AvailabilityNotifier notifier, TimeProvider clock, Guid venueId)
    {
        var tz = await db.Venues.Where(v => v.Id == venueId).Select(v => v.Timezone).FirstAsync();
        var today = VenueClock.Today(tz, clock.GetUtcNow().UtcDateTime);
        await notifier.ChangedAsync(venueId, Enumerable.Range(0, HorizonDays).Select(today.AddDays));
    }
}
