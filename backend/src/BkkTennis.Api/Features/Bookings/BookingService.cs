using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Payments;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BkkTennis.Api.Features.Bookings;

public record SlotRequest(Guid CourtId, string Start);
public record AddonRequest(Guid AddonId, int Qty = 1);
public record CreateBookingRequest(Guid VenueId, DateOnly Date, List<SlotRequest> Slots, List<AddonRequest>? Addons);

public class BookingOptions
{
    public int HoldMinutes { get; set; } = 10;
    public int MaxSlotsPerBooking { get; set; } = 4;
    public int MaxAdvanceDays { get; set; } = 14;
}

public class BookingException(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}

public class BookingService(
    AppDbContext db,
    IPaymentProvider payments,
    AvailabilityNotifier notifier,
    TimeProvider clock,
    IOptions<BookingOptions> options,
    ILogger<BookingService> log)
{
    private BookingOptions Opt => options.Value;

    public async Task<(Booking booking, Payment payment)> CreateHoldAsync(
        Guid userId, CreateBookingRequest req, string? idempotencyKey, CancellationToken ct)
    {
        if (idempotencyKey is not null)
        {
            var existing = await db.Bookings.Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.IdempotencyKey == idempotencyKey, ct);
            if (existing is not null)
                return (existing, existing.Payments.OrderByDescending(p => p.CreatedAt).First());
        }

        if (req.Slots is not { Count: > 0 })
            throw new BookingException(400, "no_slots", "เลือกอย่างน้อย 1 ช่วงเวลา");
        if (req.Slots.Count > Opt.MaxSlotsPerBooking)
            throw new BookingException(400, "too_many_slots", $"จองได้สูงสุด {Opt.MaxSlotsPerBooking} ช่วงต่อครั้ง");

        var venue = await db.Venues.AsNoTracking()
            .Include(v => v.Partner)
            .Include(v => v.Courts)
            .Include(v => v.OperatingHours)
            .Include(v => v.PriceRules)
            .Include(v => v.Addons)
            .FirstOrDefaultAsync(v => v.Id == req.VenueId && v.Status == "active", ct)
            ?? throw new BookingException(404, "venue_not_found", "ไม่พบสนาม");

        var now = clock.GetUtcNow().UtcDateTime;
        var today = VenueClock.Today(venue.Timezone, now);
        if (req.Date < today || req.Date > today.AddDays(Opt.MaxAdvanceDays))
            throw new BookingException(400, "date_out_of_range", $"จองล่วงหน้าได้ไม่เกิน {Opt.MaxAdvanceDays} วัน");

        var hours = venue.OperatingHours.FirstOrDefault(h => h.DayOfWeek == (short)req.Date.DayOfWeek)
            ?? throw new BookingException(400, "closed", "สนามปิดในวันที่เลือก");

        var hasOpenHold = await db.Bookings.AnyAsync(b =>
            b.UserId == userId && b.Status == BookingStatus.Held && b.HoldExpiresAt > now, ct);
        if (hasOpenHold)
            throw new BookingException(409, "open_hold_exists", "คุณมีรายการที่รอชำระเงินอยู่ กรุณาชำระหรือยกเลิกก่อน");

        // ---- price every slot on the server; never trust client prices
        var slots = BuildSlots(venue, hours, req.Date, req.Slots, now, BookingStatus.Held, allowInProgress: false);
        await EnsureNotBlockedAsync(slots, ct);

        // ---- add-ons
        var totalHours = slots.Sum(s => (decimal)(s.EndAt - s.StartAt).TotalHours);
        var addonLines = new List<BookingAddon>();
        foreach (var a in req.Addons ?? [])
        {
            if (a.Qty <= 0) continue;
            var addon = venue.Addons.FirstOrDefault(x => x.Id == a.AddonId && x.IsActive)
                ?? throw new BookingException(400, "addon_not_found", "ไม่พบบริการเสริม");
            addonLines.Add(new BookingAddon
            {
                AddonId = addon.Id,
                Qty = a.Qty,
                UnitPrice = addon.Price,
                LineTotal = Pricing.AddonLineTotal(addon, a.Qty, totalHours),
            });
        }

        var subtotal = slots.Sum(s => s.Price);
        var addonTotal = addonLines.Sum(a => a.LineTotal);
        var total = subtotal + addonTotal;
        var holdUntil = now.AddMinutes(Opt.HoldMinutes);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VenueId = venue.Id,
            Status = BookingStatus.Held,
            Source = "app",
            Subtotal = subtotal,
            AddonTotal = addonTotal,
            Total = total,
            Commission = Pricing.Commission(total, venue.Partner?.CommissionRate ?? 0.10m),
            HoldExpiresAt = holdUntil,
            IdempotencyKey = idempotencyKey,
            Slots = slots,
            Addons = addonLines,
        };
        db.Bookings.Add(booking);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
        {
            throw new BookingException(409, "slot_taken", "มีผู้จองช่วงเวลานี้แล้ว กรุณาเลือกเวลาอื่น");
        }

        var intent = await payments.CreatePromptPayAsync(booking.Id, total, holdUntil, ct);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Provider = intent.Provider,
            ProviderRef = intent.ProviderRef,
            Method = intent.Method,
            Amount = total,
            Status = "pending",
            QrPayload = intent.QrPayload,
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        await db.Entry(booking).ReloadAsync(ct); // pick up DB-generated booking code
        await notifier.ChangedAsync(venue.Id, [req.Date]);
        log.LogInformation("Booking {Code} held for user {User}, total {Total}", booking.Code, userId, total);
        return (booking, payment);
    }

    /// <summary>Validates requested slots against opening hours and prices them server-side.</summary>
    /// <param name="allowInProgress">Walk-ins may start a slot that began earlier in the same slot window.</param>
    public static List<BookingSlot> BuildSlots(
        Venue venue, OperatingHours hours, DateOnly date, IEnumerable<SlotRequest> requested,
        DateTime now, string status, bool allowInProgress)
    {
        var slots = new List<BookingSlot>();
        foreach (var s in requested.DistinctBy(s => (s.CourtId, s.Start)))
        {
            var court = venue.Courts.FirstOrDefault(c => c.Id == s.CourtId && c.IsActive)
                ?? throw new BookingException(400, "court_not_found", "ไม่พบคอร์ท");
            if (!TimeOnly.TryParse(s.Start, out var start))
                throw new BookingException(400, "bad_time", $"รูปแบบเวลาไม่ถูกต้อง: {s.Start}");

            var end = start.AddMinutes(hours.SlotMinutes);
            var onGrid = (start - hours.OpenTime).TotalMinutes % hours.SlotMinutes == 0;
            if (start < hours.OpenTime || end > hours.CloseTime || end <= start || !onGrid)
                throw new BookingException(400, "outside_hours", $"{s.Start} อยู่นอกเวลาเปิดให้บริการ");

            var startUtc = VenueClock.ToUtc(date, start, venue.Timezone);
            var endUtc = startUtc.AddMinutes(hours.SlotMinutes);
            if (allowInProgress ? endUtc <= now : startUtc <= now)
                throw new BookingException(400, "slot_in_past", $"{s.Start} เลยเวลาแล้ว");

            var price = Pricing.PriceFor(venue.PriceRules, court.Id, date, start, hours.SlotMinutes)
                ?? throw new BookingException(400, "no_price", $"{s.Start} ยังไม่เปิดขาย");

            slots.Add(new BookingSlot
            {
                Id = Guid.NewGuid(),
                CourtId = court.Id,
                StartAt = startUtc,
                EndAt = endUtc,
                Price = price.Price,
                Label = price.Label,
                Status = status,
            });
        }
        return slots;
    }

    /// <summary>Maintenance blocks are not covered by the DB exclusion constraint, so check them here.</summary>
    public async Task EnsureNotBlockedAsync(IEnumerable<BookingSlot> slots, CancellationToken ct)
    {
        foreach (var s in slots)
        {
            var blocked = await db.CourtBlocks.AnyAsync(b =>
                b.CourtId == s.CourtId && b.StartAt < s.EndAt && b.EndAt > s.StartAt, ct);
            if (blocked) throw new BookingException(409, "slot_blocked", "ช่วงเวลานี้ปิดปรับปรุง");
        }
    }

    /// <summary>
    /// Front-desk booking created by venue staff: paid at the counter, so it is confirmed immediately
    /// (no hold, no payment row) and carries no platform commission.
    /// </summary>
    public async Task<Booking> CreateWalkInAsync(
        Guid staffUserId, Guid venueId, DateOnly date, List<SlotRequest> requested, string? customerName, CancellationToken ct)
    {
        if (requested is not { Count: > 0 })
            throw new BookingException(400, "no_slots", "เลือกอย่างน้อย 1 ช่วงเวลา");
        if (requested.Count > Opt.MaxSlotsPerBooking)
            throw new BookingException(400, "too_many_slots", $"จองได้สูงสุด {Opt.MaxSlotsPerBooking} ช่วงต่อครั้ง");

        var venue = await db.Venues.AsNoTracking()
            .Include(v => v.Courts).Include(v => v.OperatingHours).Include(v => v.PriceRules)
            .FirstOrDefaultAsync(v => v.Id == venueId, ct)
            ?? throw new BookingException(404, "venue_not_found", "ไม่พบสนาม");

        var now = clock.GetUtcNow().UtcDateTime;
        var hours = venue.OperatingHours.FirstOrDefault(h => h.DayOfWeek == (short)date.DayOfWeek)
            ?? throw new BookingException(400, "closed", "สนามปิดในวันที่เลือก");

        var slots = BuildSlots(venue, hours, date, requested, now, BookingStatus.Confirmed, allowInProgress: true);
        await EnsureNotBlockedAsync(slots, ct);

        var subtotal = slots.Sum(s => s.Price);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = null,
            VenueId = venue.Id,
            Status = BookingStatus.Confirmed,
            Source = "walk_in",
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim(),
            Subtotal = subtotal,
            Total = subtotal,
            Commission = 0,
            Slots = slots,
        };
        db.Bookings.Add(booking);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
        {
            throw new BookingException(409, "slot_taken", "มีผู้จองช่วงเวลานี้แล้ว");
        }

        await db.Entry(booking).ReloadAsync(ct);
        await notifier.ChangedAsync(venue.Id, [date]);
        log.LogInformation("Walk-in {Code} created by staff {Staff}, total {Total}", booking.Code, staffUserId, subtotal);
        return booking;
    }

    /// <summary>Called from the payment webhook. Idempotent.</summary>
    public async Task ConfirmPaymentAsync(string provider, string providerRef, bool succeeded, string rawBody, CancellationToken ct)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.Provider == provider && p.ProviderRef == providerRef, ct)
            ?? throw new BookingException(404, "payment_not_found", "payment not found");
        if (payment.Status != "pending") return;

        var booking = await db.Bookings.Include(b => b.Slots).Include(b => b.Venue)
            .FirstAsync(b => b.Id == payment.BookingId, ct);

        payment.RawWebhook = rawBody;
        if (!succeeded)
        {
            payment.Status = "failed";
            await db.SaveChangesAsync(ct);
            return;
        }

        payment.Status = "succeeded";
        payment.PaidAt = clock.GetUtcNow().UtcDateTime;

        if (booking.Status is BookingStatus.Held or BookingStatus.Expired)
        {
            // An expired hold can still be rescued if nobody took the slot in the meantime —
            // the exclusion constraint tells us atomically.
            booking.Status = BookingStatus.Confirmed;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = payment.PaidAt.Value;
            foreach (var s in booking.Slots) s.Status = BookingStatus.Confirmed;
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
            {
                db.ChangeTracker.Clear();
                await RefundLateAsync(payment.Id, ct);
                return;
            }
        }
        else
        {
            await db.SaveChangesAsync(ct); // e.g. paid after user cancelled — refund handled by ops for now
        }

        await NotifyAsync(booking);
    }

    private async Task RefundLateAsync(Guid paymentId, CancellationToken ct)
    {
        var payment = await db.Payments.FirstAsync(p => p.Id == paymentId, ct);
        var booking = await db.Bookings.FirstAsync(b => b.Id == payment.BookingId, ct);
        payment.Status = "refunded";
        payment.PaidAt = clock.GetUtcNow().UtcDateTime;
        booking.Status = BookingStatus.Expired;
        db.Refunds.Add(new Refund { Id = Guid.NewGuid(), PaymentId = payment.Id, Amount = payment.Amount, Reason = "slot_taken_after_hold_expired" });
        await db.SaveChangesAsync(ct);
        await payments.RefundAsync(payment.ProviderRef, payment.Amount, ct);
        log.LogWarning("Late payment for {Booking} refunded: slot already taken", booking.Code);
    }

    public async Task<(Booking booking, decimal refund)> CancelAsync(Guid userId, Guid bookingId, CancellationToken ct)
    {
        var booking = await db.Bookings.Include(b => b.Slots).Include(b => b.Payments).Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId, ct)
            ?? throw new BookingException(404, "booking_not_found", "ไม่พบการจอง");

        if (booking.Status is not (BookingStatus.Held or BookingStatus.Confirmed))
            throw new BookingException(409, "not_cancellable", "รายการนี้ยกเลิกไม่ได้");

        var now = clock.GetUtcNow().UtcDateTime;
        decimal refund = 0;
        var paid = booking.Payments.FirstOrDefault(p => p.Status == "succeeded");
        if (paid is not null)
        {
            var firstStart = booking.Slots.Min(s => s.StartAt);
            refund = Pricing.RefundAmount(paid.Amount, firstStart, now, booking.Venue!.CancelFreeHours);
            if (refund > 0)
            {
                db.Refunds.Add(new Refund { Id = Guid.NewGuid(), PaymentId = paid.Id, Amount = refund, Reason = "user_cancel", Status = "pending" });
                paid.Status = "refunded";
            }
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = now;
        foreach (var s in booking.Slots) s.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        if (paid is not null && refund > 0)
            await payments.RefundAsync(paid.ProviderRef, refund, ct);

        await NotifyAsync(booking);
        return (booking, refund);
    }

    /// <summary>Releases expired holds. Returns number of bookings expired.</summary>
    public async Task<int> ExpireHoldsAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var expired = await db.Bookings.Include(b => b.Slots).Include(b => b.Venue)
            .Where(b => b.Status == BookingStatus.Held && b.HoldExpiresAt < now)
            .OrderBy(b => b.HoldExpiresAt)
            .Take(200)
            .ToListAsync(ct);
        if (expired.Count == 0) return 0;

        foreach (var b in expired)
        {
            b.Status = BookingStatus.Expired;
            b.UpdatedAt = now;
            foreach (var s in b.Slots) s.Status = BookingStatus.Expired;
        }
        await db.SaveChangesAsync(ct);

        foreach (var b in expired) await NotifyAsync(b);
        return expired.Count;
    }

    private Task NotifyAsync(Booking b)
    {
        var tz = b.Venue?.Timezone ?? "Asia/Bangkok";
        return notifier.ChangedAsync(b.VenueId, b.Slots.Select(s => DateOnly.FromDateTime(VenueClock.ToLocal(s.StartAt, tz))));
    }
}
