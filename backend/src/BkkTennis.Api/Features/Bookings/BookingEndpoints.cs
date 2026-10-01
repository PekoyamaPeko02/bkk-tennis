using System.Security.Claims;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Bookings;

public record BookingSlotDto(Guid CourtId, string CourtName, string Date, string Start, string End, decimal Price, string? Label);
public record BookingAddonDto(Guid AddonId, string Name, int Qty, decimal LineTotal);
public record PaymentDto(string Provider, string Method, decimal Amount, string Status, string? QrPayload);
public record BookingDto(
    Guid Id, string Code, string Status, Guid VenueId, string VenueName, string VenueDistrict,
    decimal Subtotal, decimal AddonTotal, decimal Total, DateTime? HoldExpiresAt, DateTime CreatedAt,
    List<BookingSlotDto> Slots, List<BookingAddonDto> Addons, PaymentDto? Payment, int CancelFreeHours);

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookings(this RouteGroupBuilder api)
    {
        api.MapPost("/bookings", async (
            CreateBookingRequest req, HttpRequest http, ClaimsPrincipal user,
            BookingService bookings, AppDbContext db, CancellationToken ct) =>
        {
            var key = http.Headers["Idempotency-Key"].FirstOrDefault();
            var (booking, _) = await bookings.CreateHoldAsync(user.UserId(), req, key, ct);
            return Results.Created($"/api/v1/me/bookings/{booking.Id}", await LoadDtoAsync(db, booking.Id, ct));
        }).RequireAuthorization().WithValidation().WithTags("Bookings");

        var me = api.MapGroup("/me/bookings").RequireAuthorization().WithTags("Bookings");

        me.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, string? scope, CancellationToken ct) =>
        {
            var uid = user.UserId();
            var ids = await db.Bookings.AsNoTracking()
                .Where(b => b.UserId == uid && b.Status != BookingStatus.Expired)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Id)
                .Take(50)
                .ToListAsync(ct);
            var list = new List<BookingDto>();
            foreach (var id in ids) list.Add((await LoadDtoAsync(db, id, ct))!);
            return Results.Ok(list);
        });

        me.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var owner = await db.Bookings.AsNoTracking().Where(b => b.Id == id).Select(b => b.UserId).FirstOrDefaultAsync(ct);
            if (owner != user.UserId()) return Results.NotFound();
            return Results.Ok(await LoadDtoAsync(db, id, ct));
        });

        me.MapPost("/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, BookingService bookings, AppDbContext db, CancellationToken ct) =>
        {
            var (booking, refund) = await bookings.CancelAsync(user.UserId(), id, ct);
            return Results.Ok(new { booking = await LoadDtoAsync(db, booking.Id, ct), refund });
        });

        return api;
    }

    public static async Task<BookingDto?> LoadDtoAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var b = await db.Bookings.AsNoTracking()
            .Include(x => x.Venue)
            .Include(x => x.Slots).ThenInclude(s => s.Court)
            .Include(x => x.Addons).ThenInclude(a => a.Addon)
            .Include(x => x.Payments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return null;

        var tz = b.Venue!.Timezone;
        var pay = b.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        return new BookingDto(
            b.Id, b.Code, b.Status, b.VenueId, b.Venue.NameEn, b.Venue.District,
            b.Subtotal, b.AddonTotal, b.Total, b.HoldExpiresAt, b.CreatedAt,
            b.Slots.OrderBy(s => s.StartAt).Select(s =>
            {
                var start = VenueClock.ToLocal(s.StartAt, tz);
                var end = VenueClock.ToLocal(s.EndAt, tz);
                return new BookingSlotDto(s.CourtId, s.Court?.Name ?? "", start.ToString("yyyy-MM-dd"),
                    start.ToString("HH:mm"), end.ToString("HH:mm"), s.Price, s.Label);
            }).ToList(),
            b.Addons.Select(a => new BookingAddonDto(a.AddonId, a.Addon?.Name ?? "", a.Qty, a.LineTotal)).ToList(),
            pay is null ? null : new PaymentDto(pay.Provider, pay.Method, pay.Amount, pay.Status,
                b.Status == BookingStatus.Held ? pay.QrPayload : null),
            b.Venue.CancelFreeHours);
    }
}
