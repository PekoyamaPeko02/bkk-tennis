using System.Security.Claims;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.ClubOs;

public record WalkInRequest(DateOnly Date, List<SlotRequest> Slots, string? CustomerName);
public record CreateBlockRequest(Guid CourtId, DateTimeOffset StartAt, DateTimeOffset EndAt, string? Reason, string? Note);

public record WalkInDto(Guid Id, string Code, string Status, string? CustomerName, decimal Total, List<BookingSlotDto> Slots);
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
        });

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
        });

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
