using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.ClubOs;

public record WalkInRequest(
    DateOnly Date, [property: MaxLength(50)] List<SlotRequest> Slots, [property: MaxLength(100)] string? CustomerName);
public record CreateBlockRequest(
    [property: NotEmpty] Guid CourtId, DateTimeOffset StartAt, DateTimeOffset EndAt,
    [property: MaxLength(40)] string? Reason, [property: MaxLength(500)] string? Note);

public record WalkInDto(Guid Id, string Code, string Status, string? CustomerName, decimal Total, List<BookingSlotDto> Slots);
public record PriceRuleRequest(
    Guid? CourtId, [property: Required, MaxLength(20)] string DayType, [property: Required, MaxLength(8)] string Start,
    [property: Required, MaxLength(8)] string End, decimal PricePerHour, [property: MaxLength(80)] string? Label, int Priority);
public record PriceRuleDto(
    Guid Id, Guid? CourtId, string? CourtName, string DayType, string Start, string End, decimal PricePerHour, string? Label, int Priority);
public record CourtBlockDto(Guid Id, Guid CourtId, string CourtName, DateTime StartAt, DateTime EndAt, string Reason, string? Note);
public record DayBookingDto(
    Guid Id, string Code, string Status, string Source, string? CustomerName, decimal Total, List<BookingSlotDto> Slots);

/// <summary>Venue-side (ClubOS) endpoints. Every route is scoped to a venue the caller is a member of.</summary>
public static class ClubOsEndpoints
{
    private static readonly string[] BlockReasons = ["maintenance", "cleaning", "event", "other"];

    public static RouteGroupBuilder MapClubOs(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/clubos/venues/{venueId:guid}")
            .RequireAuthorization(p => p.RequireRole(Roles.PartnerStaff, Roles.PartnerOwner))
            .AddEndpointFilter(RequireVenueMember)
            .WithTags("ClubOS");

        g.MapGet("/availability", async (Guid venueId, DateOnly date, AvailabilityService availability, CancellationToken ct) =>
            await availability.ForVenueAsync(venueId, date, ct) is { } dto ? Results.Ok(dto) : Results.NotFound());

        g.MapGet("/bookings", async (Guid venueId, DateOnly date, AppDbContext db, CancellationToken ct) =>
        {
            var tz = await db.Venues.Where(v => v.Id == venueId).Select(v => v.Timezone).FirstAsync(ct);
            var dayStart = VenueClock.ToUtc(date, TimeOnly.MinValue, tz);
            var dayEnd = dayStart.AddDays(1);
            var bookings = await db.Bookings.AsNoTracking()
                .Include(b => b.Slots).ThenInclude(s => s.Court)
                .Where(b => b.VenueId == venueId
                            && b.Status != BookingStatus.Expired
                            && b.Slots.Any(s => s.StartAt >= dayStart && s.StartAt < dayEnd))
                .OrderBy(b => b.Slots.Min(s => s.StartAt))
                .ToListAsync(ct);
            return Results.Ok(bookings.Select(b => new DayBookingDto(
                b.Id, b.Code, b.Status, b.Source, b.CustomerName, b.Total, ToSlotDtos(b, tz))));
        });

        g.MapPost("/bookings/{bookingId:guid}/check-in", async (
            Guid venueId, Guid bookingId, BookingService bookings, AppDbContext db, CancellationToken ct) =>
        {
            var booking = await bookings.CheckInAsync(venueId, bookingId, ct);
            var tz = booking.Venue?.Timezone ?? "Asia/Bangkok";
            var full = await db.Bookings.AsNoTracking().Include(b => b.Slots).ThenInclude(s => s.Court)
                .FirstAsync(b => b.Id == booking.Id, ct);
            return Results.Ok(new DayBookingDto(
                full.Id, full.Code, full.Status, full.Source, full.CustomerName, full.Total, ToSlotDtos(full, tz)));
        });

        g.MapPost("/bookings/{bookingId:guid}/no-show", async (
            Guid venueId, Guid bookingId, BookingService bookings, AppDbContext db, CancellationToken ct) =>
        {
            var booking = await bookings.MarkNoShowAsync(venueId, bookingId, ct);
            var tz = booking.Venue?.Timezone ?? "Asia/Bangkok";
            var full = await db.Bookings.AsNoTracking().Include(b => b.Slots).ThenInclude(s => s.Court)
                .FirstAsync(b => b.Id == booking.Id, ct);
            return Results.Ok(new DayBookingDto(
                full.Id, full.Code, full.Status, full.Source, full.CustomerName, full.Total, ToSlotDtos(full, tz)));
        });

        // Front-desk lookup: staff type/scan the booking code shown in the player app.
        g.MapGet("/bookings/by-code/{code}", async (Guid venueId, string code, AppDbContext db, CancellationToken ct) =>
        {
            var venue = await db.Venues.AsNoTracking().FirstAsync(v => v.Id == venueId, ct);
            var b = await db.Bookings.AsNoTracking().Include(x => x.Slots).ThenInclude(s => s.Court)
                .FirstOrDefaultAsync(x => x.VenueId == venueId && x.Code == code.Trim().ToUpperInvariant(), ct);
            return b is null ? Results.NotFound()
                : Results.Ok(new DayBookingDto(b.Id, b.Code, b.Status, b.Source, b.CustomerName, b.Total, ToSlotDtos(b, venue.Timezone)));
        });

        g.MapPost("/walk-ins", async (
            Guid venueId, WalkInRequest req, ClaimsPrincipal user, BookingService bookings, AppDbContext db, CancellationToken ct) =>
        {
            var booking = await bookings.CreateWalkInAsync(user.UserId(), venueId, req.Date, req.Slots, req.CustomerName, ct);
            var tz = await db.Venues.Where(v => v.Id == venueId).Select(v => v.Timezone).FirstAsync(ct);
            var full = await db.Bookings.AsNoTracking().Include(b => b.Slots).ThenInclude(s => s.Court)
                .FirstAsync(b => b.Id == booking.Id, ct);
            return Results.Created($"/api/v1/clubos/venues/{venueId}/bookings?date={req.Date:yyyy-MM-dd}",
                new WalkInDto(full.Id, full.Code, full.Status, full.CustomerName, full.Total, ToSlotDtos(full, tz)));
        }).WithValidation();

        g.MapGet("/blocks", async (Guid venueId, DateOnly date, AppDbContext db, CancellationToken ct) =>
        {
            var tz = await db.Venues.Where(v => v.Id == venueId).Select(v => v.Timezone).FirstAsync(ct);
            var dayStart = VenueClock.ToUtc(date, TimeOnly.MinValue, tz);
            var dayEnd = dayStart.AddDays(1);
            var blocks = await (from b in db.CourtBlocks.AsNoTracking()
                                join c in db.Courts on b.CourtId equals c.Id
                                where c.VenueId == venueId && b.StartAt < dayEnd && b.EndAt > dayStart
                                orderby b.StartAt
                                select new CourtBlockDto(b.Id, b.CourtId, c.Name, b.StartAt, b.EndAt, b.Reason, b.Note))
                .ToListAsync(ct);
            return Results.Ok(blocks);
        });

        g.MapPost("/blocks", async (
            Guid venueId, CreateBlockRequest req, ClaimsPrincipal user, AppDbContext db, AvailabilityNotifier notifier, CancellationToken ct) =>
        {
            var reason = req.Reason ?? "maintenance";
            if (!BlockReasons.Contains(reason))
                throw new BookingException(400, "bad_reason", "เหตุผลไม่ถูกต้อง");
            var start = req.StartAt.UtcDateTime;
            var end = req.EndAt.UtcDateTime;
            if (end <= start)
                throw new BookingException(400, "bad_range", "เวลาสิ้นสุดต้องหลังเวลาเริ่ม");

            var court = await db.Courts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.CourtId && c.VenueId == venueId, ct)
                ?? throw new BookingException(404, "court_not_found", "ไม่พบคอร์ท");

            // Blocks are not covered by the exclusion constraint: refuse to block over live bookings
            // (staff must cancel those first) instead of silently double-booking the court.
            var conflicts = await db.BookingSlots.AsNoTracking()
                .Where(s => s.CourtId == court.Id
                            && BookingStatus.Active.Contains(s.Status)
                            && s.StartAt < end && s.EndAt > start)
                .Select(s => s.BookingId)
                .Distinct()
                .Join(db.Bookings, id => id, b => b.Id, (_, b) => b.Code)
                .ToListAsync(ct);
            if (conflicts.Count > 0)
                throw new BookingException(409, "block_conflict",
                    $"มีการจองในช่วงเวลานี้แล้ว ({string.Join(", ", conflicts)}) กรุณายกเลิกก่อน");

            var block = new CourtBlock
            {
                Id = Guid.NewGuid(),
                CourtId = court.Id,
                StartAt = start,
                EndAt = end,
                Reason = reason,
                Note = req.Note,
                CreatedBy = user.UserId(),
            };
            db.CourtBlocks.Add(block);
            await db.SaveChangesAsync(ct);

            await NotifyRangeAsync(db, notifier, venueId, start, end);
            return Results.Created($"/api/v1/clubos/venues/{venueId}/blocks/{block.Id}",
                new CourtBlockDto(block.Id, court.Id, court.Name, start, end, reason, block.Note));
        }).WithValidation();

        g.MapDelete("/blocks/{blockId:guid}", async (
            Guid venueId, Guid blockId, AppDbContext db, AvailabilityNotifier notifier, CancellationToken ct) =>
        {
            var block = await (from b in db.CourtBlocks
                               join c in db.Courts on b.CourtId equals c.Id
                               where b.Id == blockId && c.VenueId == venueId
                               select b).FirstOrDefaultAsync(ct);
            if (block is null) return Results.NotFound();
            db.CourtBlocks.Remove(block);
            await db.SaveChangesAsync(ct);
            await NotifyRangeAsync(db, notifier, venueId, block.StartAt, block.EndAt);
            return Results.NoContent();
        });

        // ---- prices. Reading is open to staff; changing prices is owner-only.
        // Existing bookings keep the price they were sold at (booking_slots.price is a snapshot).
        g.MapGet("/prices", async (Guid venueId, AppDbContext db, CancellationToken ct) =>
        {
            var rules = await db.PriceRules.AsNoTracking().Where(r => r.VenueId == venueId).ToListAsync(ct);
            var courts = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            return Results.Ok(rules
                .OrderBy(r => r.CourtId.HasValue).ThenBy(r => r.StartTime).ThenByDescending(r => r.Priority)
                .Select(r => ToDto(r, courts)));
        });

        g.MapPost("/prices", async (Guid venueId, PriceRuleRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var rule = new PriceRule { Id = Guid.NewGuid(), VenueId = venueId };
            await ApplyAsync(db, venueId, rule, req, ct);
            db.PriceRules.Add(rule);
            await db.SaveChangesAsync(ct);
            var courts = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            return Results.Created($"/api/v1/clubos/venues/{venueId}/prices/{rule.Id}", ToDto(rule, courts));
        }).AddEndpointFilter(RequireVenueOwner).WithValidation();

        g.MapPut("/prices/{ruleId:guid}", async (Guid venueId, Guid ruleId, PriceRuleRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var rule = await db.PriceRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.VenueId == venueId, ct);
            if (rule is null) return Results.NotFound();
            await ApplyAsync(db, venueId, rule, req, ct);
            await db.SaveChangesAsync(ct);
            var courts = await db.Courts.AsNoTracking().Where(c => c.VenueId == venueId).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            return Results.Ok(ToDto(rule, courts));
        }).AddEndpointFilter(RequireVenueOwner).WithValidation();

        g.MapDelete("/prices/{ruleId:guid}", async (Guid venueId, Guid ruleId, AppDbContext db, CancellationToken ct) =>
        {
            var rule = await db.PriceRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.VenueId == venueId, ct);
            if (rule is null) return Results.NotFound();
            db.PriceRules.Remove(rule);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).AddEndpointFilter(RequireVenueOwner);

        return api;
    }

    private static async ValueTask<object?> RequireVenueMember(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var http = ctx.HttpContext;
        var venueId = Guid.Parse((string)http.Request.RouteValues["venueId"]!);
        var uid = http.User.UserId();
        var db = http.RequestServices.GetRequiredService<AppDbContext>();
        var isMember = await db.VenueMembers.AnyAsync(m => m.VenueId == venueId && m.UserId == uid, http.RequestAborted);
        return isMember ? await next(ctx) : Results.Forbid();
    }

    private static async ValueTask<object?> RequireVenueOwner(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var http = ctx.HttpContext;
        var venueId = Guid.Parse((string)http.Request.RouteValues["venueId"]!);
        var uid = http.User.UserId();
        var db = http.RequestServices.GetRequiredService<AppDbContext>();
        var isOwner = await db.VenueMembers.AnyAsync(m => m.VenueId == venueId && m.UserId == uid && m.Role == "owner", http.RequestAborted);
        return isOwner ? await next(ctx) : Results.Forbid();
    }

    private static readonly string[] DayTypes = ["weekday", "weekend", "any"];

    private static PriceRuleDto ToDto(PriceRule r, Dictionary<Guid, string> courts) =>
        new(r.Id, r.CourtId, r.CourtId is { } c && courts.TryGetValue(c, out var n) ? n : null,
            r.DayType, r.StartTime.ToString("HH:mm"), r.EndTime.ToString("HH:mm"), r.PricePerHour, r.Label, r.Priority);

    /// <summary>Validates the request, copies it onto <paramref name="rule"/> and rejects ambiguous overlaps.</summary>
    private static async Task ApplyAsync(AppDbContext db, Guid venueId, PriceRule rule, PriceRuleRequest req, CancellationToken ct)
    {
        if (!DayTypes.Contains(req.DayType))
            throw new BookingException(400, "bad_day_type", "day_type ต้องเป็น weekday, weekend หรือ any");
        if (!TimeOnly.TryParse(req.Start, out var start) || !TimeOnly.TryParse(req.End, out var end))
            throw new BookingException(400, "bad_time", "รูปแบบเวลาไม่ถูกต้อง (HH:mm)");
        if (end <= start)
            throw new BookingException(400, "bad_range", "เวลาสิ้นสุดต้องหลังเวลาเริ่ม");
        if (req.PricePerHour < 0 || req.PricePerHour > 100_000)
            throw new BookingException(400, "bad_price", "ราคาไม่ถูกต้อง");
        if (req.CourtId is { } courtId && !await db.Courts.AnyAsync(c => c.Id == courtId && c.VenueId == venueId, ct))
            throw new BookingException(404, "court_not_found", "ไม่พบคอร์ท");

        rule.CourtId = req.CourtId;
        rule.DayType = req.DayType;
        rule.StartTime = start;
        rule.EndTime = end;
        rule.PricePerHour = decimal.Round(req.PricePerHour, 2);
        rule.Label = string.IsNullOrWhiteSpace(req.Label) ? null : req.Label.Trim();
        rule.Priority = req.Priority;

        var others = await db.PriceRules.AsNoTracking().Where(r => r.VenueId == venueId && r.Id != rule.Id).ToListAsync(ct);
        var clash = others.FirstOrDefault(o => Pricing.AreAmbiguous(o, rule));
        if (clash is not null)
            throw new BookingException(409, "price_rule_ambiguous",
                $"ทับกับกฎราคาเดิม ({clash.StartTime:HH:mm}-{clash.EndTime:HH:mm}) ที่ priority เท่ากัน กรุณาเปลี่ยน priority");
    }

    private static List<BookingSlotDto> ToSlotDtos(Booking b, string tz) =>
        b.Slots.OrderBy(s => s.StartAt).Select(s =>
        {
            var start = VenueClock.ToLocal(s.StartAt, tz);
            var end = VenueClock.ToLocal(s.EndAt, tz);
            return new BookingSlotDto(s.CourtId, s.Court?.Name ?? "", DateOnly.FromDateTime(start).ToString("yyyy-MM-dd"),
                start.ToString("HH:mm"), end.ToString("HH:mm"), s.Price, s.Label);
        }).ToList();

    private static async Task NotifyRangeAsync(AppDbContext db, AvailabilityNotifier notifier, Guid venueId, DateTime startUtc, DateTime endUtc)
    {
        var tz = await db.Venues.Where(v => v.Id == venueId).Select(v => v.Timezone).FirstAsync();
        var first = DateOnly.FromDateTime(VenueClock.ToLocal(startUtc, tz));
        var last = DateOnly.FromDateTime(VenueClock.ToLocal(endUtc, tz));
        var days = Enumerable.Range(0, Math.Min(last.DayNumber - first.DayNumber + 1, 31)).Select(i => first.AddDays(i));
        await notifier.ChangedAsync(venueId, days);
    }
}
