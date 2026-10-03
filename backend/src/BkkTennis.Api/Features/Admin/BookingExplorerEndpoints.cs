using System.Globalization;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.Api.Features.Admin;

/// <summary>PaymentState: paid | pending | failed | refunded | counter (walk-in, paid at the desk) | none. Date/Start/End are the play time at the venue.</summary>
public record AdminBookingDto(
    Guid Id, string Code, string Status, string Source, Guid VenueId, string VenueName, string? CustomerName, string? CustomerPhone,
    string PaymentState, string? Date, string? Start, string? End, List<string> Courts, decimal Total, decimal Commission, DateTime CreatedAt);
public record AdminBookingPageDto(int Total, int Page, int PageSize, List<AdminBookingDto> Items);

/// <summary>Platform-wide booking explorer: filter by venue / play date / status / source / text, paged JSON or a CSV export.</summary>
public static class BookingExplorerEndpoints
{
    private const string PlatformTimezone = "Asia/Bangkok";
    private const int MaxPageSize = 100;
    private const int MaxCsvRows = 20_000;
    private static readonly string[] Statuses =
        [BookingStatus.Held, BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed, BookingStatus.Expired, BookingStatus.Cancelled, BookingStatus.NoShow];
    private static readonly string[] Sources = ["app", "walk_in"];

    public static void MapBookingExplorer(this RouteGroupBuilder g)
    {
        g.MapGet("/bookings", async (
            [AsParameters] BookingFilter f, int? page, int? pageSize, AppDbContext db, CancellationToken ct) =>
        {
            var query = Filter(db, f);
            var size = Math.Clamp(pageSize ?? 25, 1, MaxPageSize);
            var pageNo = Math.Max(page ?? 1, 1);
            var total = await query.CountAsync(ct);
            var rows = await Ordered(query).Skip((pageNo - 1) * size).Take(size).ToListAsync(ct);
            return Results.Ok(new AdminBookingPageDto(total, pageNo, size, await ToDtosAsync(db, rows, ct)));
        });

        g.MapGet("/bookings.csv", async (
            [AsParameters] BookingFilter f, AppDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            var query = Filter(db, f);
            var total = await query.CountAsync(ct);
            if (total > MaxCsvRows)
                throw new BookingException(400, "too_many_rows", $"ผลลัพธ์มี {total:N0} รายการ (สูงสุด {MaxCsvRows:N0}) กรุณากรองให้แคบลง");
            var dtos = await ToDtosAsync(db, await Ordered(query).ToListAsync(ct), ct);

            var sb = new StringBuilder("﻿"); // BOM so Excel reads the Thai text as UTF-8
            sb.Append("code,status,source,venue,customer,phone,payment,date,start,end,courts,total,commission,created_at\r\n");
            foreach (var b in dtos)
            {
                Row(sb, b.Code, b.Status, b.Source, b.VenueName, b.CustomerName, b.CustomerPhone, b.PaymentState, b.Date, b.Start, b.End,
                    string.Join(" + ", b.Courts), b.Total.ToString("0.00", CultureInfo.InvariantCulture), b.Commission.ToString("0.00", CultureInfo.InvariantCulture),
                    VenueClock.ToLocal(b.CreatedAt, PlatformTimezone).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            }
            var stamp = VenueClock.Today(PlatformTimezone, clock.GetUtcNow().UtcDateTime).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", $"bookings-{stamp}.csv");
        });
    }

    public record BookingFilter(Guid? VenueId, string? Status, string? Source, DateOnly? From, DateOnly? To, string? Q);

    private static IQueryable<Booking> Filter(AppDbContext db, BookingFilter f)
    {
        if (f.Status is not null && !Statuses.Contains(f.Status))
            throw new BookingException(400, "bad_status", "สถานะไม่ถูกต้อง");
        if (f.Source is not null && !Sources.Contains(f.Source))
            throw new BookingException(400, "bad_source", "source ต้องเป็น app หรือ walk_in");
        if (f.From is { } from && f.To is { } to && to < from)
            throw new BookingException(400, "bad_range", "วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่ม");

        var q = db.Bookings.AsNoTracking().AsQueryable();
        if (f.VenueId is { } venueId) q = q.Where(b => b.VenueId == venueId);
        if (f.Status is not null) q = q.Where(b => b.Status == f.Status);
        if (f.Source is not null) q = q.Where(b => b.Source == f.Source);
        // by play date (first slot), venue-local like the rest of the platform
        if (f.From is { } d1)
        {
            var start = VenueClock.ToUtc(d1, TimeOnly.MinValue, PlatformTimezone);
            q = q.Where(b => b.Slots.Min(s => s.StartAt) >= start);
        }
        if (f.To is { } d2)
        {
            var end = VenueClock.ToUtc(d2.AddDays(1), TimeOnly.MinValue, PlatformTimezone);
            q = q.Where(b => b.Slots.Min(s => s.StartAt) < end);
        }
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim();
            var like = "%" + term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            // The 3-argument overload sets ESCAPE '\'; the 2-argument one emits ESCAPE '' and ignores our backslashes.
            q = q.Where(b => EF.Functions.ILike(b.Code, like, "\\")
                             || (b.CustomerName != null && EF.Functions.ILike(b.CustomerName, like, "\\"))
                             || db.Users.Any(u => u.Id == b.UserId && u.DisplayName != null && EF.Functions.ILike(u.DisplayName, like, "\\")));
        }
        return q;
    }

    private static IQueryable<Booking> Ordered(IQueryable<Booking> q) => q.OrderByDescending(b => b.CreatedAt).ThenBy(b => b.Id);

    private static async Task<List<AdminBookingDto>> ToDtosAsync(AppDbContext db, List<Booking> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return [];
        var ids = rows.Select(b => b.Id).ToList();
        var full = await db.Bookings.AsNoTracking().Include(b => b.Slots).Include(b => b.Payments).Include(b => b.Venue)
            .Where(b => ids.Contains(b.Id)).ToDictionaryAsync(b => b.Id, ct);
        var userIds = rows.Where(b => b.UserId != null).Select(b => b.UserId!.Value).Distinct().ToList();
        var users = await db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, ct);
        var courtIds = full.Values.SelectMany(b => b.Slots.Select(s => s.CourtId)).Distinct().ToList();
        var courts = await db.Courts.AsNoTracking().Where(c => courtIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return rows.Select(r =>
        {
            var b = full[r.Id];
            var tz = b.Venue?.Timezone ?? PlatformTimezone;
            var slots = b.Slots.OrderBy(s => s.StartAt).ToList();
            var user = b.UserId is { } uid && users.TryGetValue(uid, out var u) ? u : null;
            string? date = null, start = null, end = null;
            if (slots.Count > 0)
            {
                var first = VenueClock.ToLocal(slots[0].StartAt, tz);
                date = DateOnly.FromDateTime(first).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                start = first.ToString("HH:mm", CultureInfo.InvariantCulture);
                end = VenueClock.ToLocal(slots.Max(s => s.EndAt), tz).ToString("HH:mm", CultureInfo.InvariantCulture);
            }
            return new AdminBookingDto(
                b.Id, b.Code, b.Status, b.Source, b.VenueId, b.Venue?.NameEn ?? "", b.CustomerName ?? user?.DisplayName, PhoneMask.Mask(user?.Phone),
                PaymentState(b), date, start, end,
                slots.Select(s => courts.GetValueOrDefault(s.CourtId, "")).Distinct().ToList(), b.Total, b.Commission, b.CreatedAt);
        }).ToList();
    }

    private static string PaymentState(Booking b)
    {
        if (b.Source == "walk_in") return "counter";
        var latest = b.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        return latest?.Status switch { null => "none", "succeeded" => "paid", var other => other };
    }

    private static void Row(StringBuilder sb, params string?[] cells)
    {
        sb.AppendJoin(',', cells.Select(Cell)).Append("\r\n");
    }

    /// <summary>RFC 4180 quoting plus a guard against spreadsheet formula injection (customer names are user input).</summary>
    private static string Cell(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v[0] is '=' or '+' or '-' or '@' or '\t' or '\r') v = "'" + v;
        return v.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? "\"" + v.Replace("\"", "\"\"") + "\"" : v;
    }
}
