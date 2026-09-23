using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests.Infrastructure;

/// <summary>
/// A private, hidden venue created straight in the database so a test controls every booking status and never
/// collides with other tests' data (hidden ⇒ absent from search and from booking-by-API, but partners can still use it).
/// Two courts open 06:00–22:00 (60 min): A = hard indoor, 400/h before 17:00 and 600/h after; B = clay outdoor, 500/h flat.
/// </summary>
public sealed class ScheduleVenue
{
    private readonly ApiFactory _api;

    public Guid Id { get; } = Guid.NewGuid();
    public Guid CourtA { get; } = Guid.NewGuid();
    public Guid CourtB { get; } = Guid.NewGuid();
    public TestUser Staff { get; private set; } = null!;

    private ScheduleVenue(ApiFactory api) => _api = api;

    public string Url(DateOnly? date = null) =>
        $"/api/v1/clubos/venues/{Id}/schedule" + (date is { } d ? $"?date={d:yyyy-MM-dd}" : "");

    public static async Task<ScheduleVenue> CreateAsync(ApiFactory api, bool withHours = true)
    {
        var v = new ScheduleVenue(api);
        var slug = "sched-" + v.Id.ToString("N")[..12];
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({v.Id}, '10000000-0000-0000-0000-000000000001', {slug}, 'ทดสอบ', 'Schedule Test', 'Watthana', 'x', 13.7, 100.5, 'hidden')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO courts (id, venue_id, name, surface, indoor, air_con, floodlit, sort_order) VALUES
                  ({v.CourtA}, {v.Id}, 'คอร์ท A', 'hard', true, true, true, 1),
                  ({v.CourtB}, {v.Id}, 'คอร์ท B', 'clay', false, false, false, 2)
                """);
            if (withHours)
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO operating_hours (venue_id, day_of_week, open_time, close_time, slot_minutes)
                    SELECT {v.Id}, d, '06:00', '22:00', 60 FROM generate_series(0, 6) AS d
                    """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO price_rules (venue_id, court_id, day_type, start_time, end_time, price_per_hour, label, priority) VALUES
                  ({v.Id}, NULL, 'any', '06:00', '17:00', 400, 'Day', 0),
                  ({v.Id}, NULL, 'any', '17:00', '22:00', 600, 'Evening', 10),
                  ({v.Id}, {v.CourtB}, 'any', '06:00', '22:00', 500, 'Clay', 0)
                """);
        });
        v.Staff = await api.LoginPartnerAsync(v.Id);
        return v;
    }

    public Task DeleteHoursForAsync(DayOfWeek day) => _api.WithDb(db =>
        db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM operating_hours WHERE venue_id = {Id} AND day_of_week = {(short)day}"));

    public Task<int> SetCourtActiveAsync(Guid court, bool active) => _api.WithScope(async sp =>
        await sp.GetRequiredService<BkkTennis.Api.Data.AppDbContext>().Database
            .ExecuteSqlInterpolatedAsync($"UPDATE courts SET is_active = {active} WHERE id = {court}"));

    public Task DeletePriceRulesAsync(params string[] labels) => _api.WithDb(async db =>
    {
        foreach (var label in labels)
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM price_rules WHERE venue_id = {Id} AND label = {label}");
    });

    /// <summary>An extra payment row with an explicit created_at, to test which payment is "latest".</summary>
    public Task AddPaymentAsync(Guid booking, string status, DateTime createdAt) => _api.WithDb(db =>
    {
        var providerRef = "test_" + Guid.NewGuid().ToString("N");
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO payments (booking_id, provider, provider_ref, method, amount, status, created_at)
            VALUES ({booking}, 'mock', {providerRef}, 'promptpay', 1, {status}, {createdAt})
            """);
    });

    /// <summary>Adds another slot to an existing booking (e.g. on a second day).</summary>
    public Task AddSlotAsync(Guid booking, Guid court, DateOnly date, string start, string status = "confirmed", decimal price = 400)
    {
        var s = Utc(date, start);
        var e = s.AddHours(1);
        return _api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO booking_slots (booking_id, court_id, start_at, end_at, price, status)
            VALUES ({booking}, {court}, {s}, {e}, {price}, {status})
            """));
    }

    public static DateTime Utc(DateOnly date, string time) => VenueClock.ToUtc(date, TimeOnly.Parse(time), "Asia/Bangkok");

    /// <summary>Inserts a booking (and its slots / payment / add-ons) directly. Slot statuses mirror the booking status, like production.</summary>
    public async Task<Guid> AddBookingAsync(
        Guid court, DateOnly date, string[] starts, string status = "confirmed", string source = "app",
        string? customerName = null, string? userDisplayName = null, string? userPhone = null,
        string? payment = "succeeded", decimal? total = null, decimal addonTotal = 0, DateTime? holdExpiresAt = null,
        (string Name, int Qty, decimal Unit)? addon = null)
    {
        var id = Guid.NewGuid();
        Guid? userId = null;
        var prices = starts.Select(s => court == CourtB ? 500m : TimeOnly.Parse(s) >= new TimeOnly(17, 0) ? 600m : 400m).ToArray();
        var subtotal = prices.Sum();
        var bookingTotal = total ?? subtotal + addonTotal;

        await _api.WithDb(async db =>
        {
            if (userDisplayName is not null || userPhone is not null)
            {
                userId = Guid.NewGuid();
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO users (id, phone, display_name, role) VALUES ({userId}, {userPhone}, {userDisplayName}, 'player')
                    """);
            }
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO bookings (id, user_id, venue_id, status, source, customer_name, subtotal, addon_total, total, hold_expires_at)
                VALUES ({id}, {userId}, {Id}, {status}, {source}, {customerName}, {subtotal}, {addonTotal}, {bookingTotal}, {holdExpiresAt})
                """);
            for (var i = 0; i < starts.Length; i++)
            {
                var slotId = Guid.NewGuid();
                var start = Utc(date, starts[i]);
                var end = start.AddHours(1);
                var price = prices[i];
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO booking_slots (id, booking_id, court_id, start_at, end_at, price, status)
                    VALUES ({slotId}, {id}, {court}, {start}, {end}, {price}, {status})
                    """);
            }
            if (payment is not null)
            {
                var paymentId = Guid.NewGuid();
                var providerRef = "test_" + Guid.NewGuid().ToString("N");
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO payments (id, booking_id, provider, provider_ref, method, amount, status)
                    VALUES ({paymentId}, {id}, 'mock', {providerRef}, 'promptpay', {bookingTotal}, {payment})
                    """);
            }
            if (addon is { } a)
            {
                var addonId = Guid.NewGuid();
                var line = a.Unit * a.Qty;
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO addons (id, venue_id, name, price, unit) VALUES ({addonId}, {Id}, {a.Name}, {a.Unit}, 'per_booking')
                    """);
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO booking_addons (booking_id, addon_id, qty, unit_price, line_total) VALUES ({id}, {addonId}, {a.Qty}, {a.Unit}, {line})
                    """);
            }
        });
        return id;
    }

    public async Task<Guid> AddBlockAsync(Guid court, DateOnly date, string from, string to, string reason = "maintenance", string? note = null)
    {
        var id = Guid.NewGuid();
        var start = Utc(date, from);
        var end = Utc(date, to);
        await _api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO court_blocks (id, court_id, start_at, end_at, reason, note) VALUES ({id}, {court}, {start}, {end}, {reason}, {note})
            """));
        return id;
    }
}
