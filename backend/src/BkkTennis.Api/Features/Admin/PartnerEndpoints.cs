using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BkkTennis.Api.Features.Admin;

public record PartnerVenueDto(Guid Id, string Slug, string NameTh, string NameEn, string Status);
public record PartnerDto(
    Guid Id, string LegalName, string? TaxId, string? BankName, string? BankAccount, decimal CommissionRate, string Status,
    DateTime CreatedAt, string? OwnerEmail, string? OwnerName, List<PartnerVenueDto> Venues);

public record NewPartnerOwner(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MaxLength(80)] string DisplayName,
    [property: Required, MinLength(10), MaxLength(128), MaxUtf8Bytes(72)] string Password);
public record NewPartnerVenue(
    [property: Required, MaxLength(60)] string Slug, [property: Required, MaxLength(120)] string NameTh,
    [property: Required, MaxLength(120)] string NameEn, [property: Required, MaxLength(80)] string District,
    [property: Required, MaxLength(240)] string Address, double Lat, double Lng);
public record CreatePartnerRequest(
    [property: Required, MaxLength(160)] string LegalName, [property: MaxLength(20)] string? TaxId,
    [property: MaxLength(60)] string? BankName, [property: MaxLength(40)] string? BankAccount,
    decimal? CommissionRate, [property: Required] NewPartnerOwner Owner, [property: Required] NewPartnerVenue Venue);

/// <summary>Partner onboarding: list, create (pending partner + owner login + draft venue), approve, suspend.</summary>
public static partial class PartnerEndpoints
{
    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRx();

    public static void MapPartners(this RouteGroupBuilder g)
    {
        g.MapGet("/partners", async (string? status, AppDbContext db, CancellationToken ct) =>
        {
            if (status is not null && status is not ("pending" or "active" or "suspended"))
                throw new BookingException(400, "bad_status", "status ต้องเป็น pending, active หรือ suspended");
            var q = db.Partners.AsNoTracking();
            if (status is not null) q = q.Where(p => p.Status == status);
            var partners = await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
            return Results.Ok(await ToDtosAsync(db, partners, ct));
        });

        g.MapPost("/partners", async (CreatePartnerRequest req, AppDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            var rate = req.CommissionRate ?? 0.10m;
            if (rate < 0 || rate > 0.5m) throw new BookingException(400, "bad_commission", "ค่าคอมมิชชั่นต้องอยู่ระหว่าง 0-50%");
            var legalName = req.LegalName.Trim();
            var email = req.Owner.Email.Trim().ToLowerInvariant();
            var slug = req.Venue.Slug.Trim();
            if (legalName.Length == 0) throw new BookingException(400, "bad_name", "กรุณาระบุชื่อนิติบุคคล");
            if (!SlugRx().IsMatch(slug)) throw new BookingException(400, "bad_slug", "slug ใช้ได้เฉพาะ a-z 0-9 และ - (เช่น my-tennis-club)");
            if (req.Venue.Lat is < -90 or > 90 || req.Venue.Lng is < -180 or > 180)
                throw new BookingException(400, "bad_location", "พิกัดไม่ถูกต้อง");
            if (await db.Users.AnyAsync(u => u.Email == email, ct))
                throw new BookingException(409, "email_taken", "อีเมลนี้ถูกใช้แล้ว");
            if (await db.Venues.AnyAsync(v => v.Slug == slug, ct))
                throw new BookingException(409, "slug_taken", "slug นี้ถูกใช้แล้ว");

            var now = clock.GetUtcNow().UtcDateTime;
            var partner = new Partner
            {
                Id = Guid.NewGuid(), LegalName = legalName, TaxId = Blank(req.TaxId), BankName = Blank(req.BankName),
                BankAccount = Blank(req.BankAccount), CommissionRate = rate, Status = "pending", CreatedAt = now,
            };
            var owner = new User
            {
                Id = Guid.NewGuid(), Email = email, DisplayName = req.Owner.DisplayName.Trim(), Role = Roles.PartnerOwner,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Owner.Password, workFactor: 10), CreatedAt = now,
            };
            var venue = new Venue
            {
                Id = Guid.NewGuid(), PartnerId = partner.Id, Slug = slug, NameTh = req.Venue.NameTh.Trim(), NameEn = req.Venue.NameEn.Trim(),
                District = req.Venue.District.Trim(), Address = req.Venue.Address.Trim(), Lat = req.Venue.Lat, Lng = req.Venue.Lng,
                Status = "draft", CreatedAt = now,
            };
            db.Partners.Add(partner);
            db.Users.Add(owner);
            db.Venues.Add(venue);
            db.VenueMembers.Add(new VenueMember { VenueId = venue.Id, UserId = owner.Id, Role = "owner" });
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Lost a race with another create using the same email/slug.
                throw new BookingException(409, "conflict", "อีเมลหรือ slug นี้ถูกใช้แล้ว");
            }
            var dto = (await ToDtosAsync(db, [partner], ct))[0];
            return Results.Created($"/api/v1/admin/partners/{partner.Id}", dto);
        }).WithValidation();

        // pending → active (publishes the partner's draft venues) and suspended → active (venues keep their state).
        g.MapPost("/partners/{id:guid}/approve", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (partner is null) return Results.NotFound();
            if (partner.Status == "active") throw new BookingException(409, "bad_transition", "พาร์ทเนอร์นี้เปิดใช้งานอยู่แล้ว");
            partner.Status = "active";
            var drafts = await db.Venues.Where(v => v.PartnerId == id && v.Status == "draft").ToListAsync(ct);
            foreach (var v in drafts) v.Status = "active";
            await db.SaveChangesAsync(ct);
            return Results.Ok((await ToDtosAsync(db, [partner], ct))[0]);
        });

        // Hides every venue of the partner from search/detail/availability and blocks new bookings; existing bookings stay valid.
        g.MapPost("/partners/{id:guid}/suspend", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (partner is null) return Results.NotFound();
            if (partner.Status != "active") throw new BookingException(409, "bad_transition", "ระงับได้เฉพาะพาร์ทเนอร์ที่เปิดใช้งานอยู่");
            partner.Status = "suspended";
            await db.SaveChangesAsync(ct);
            return Results.Ok((await ToDtosAsync(db, [partner], ct))[0]);
        });
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static async Task<List<PartnerDto>> ToDtosAsync(AppDbContext db, List<Partner> partners, CancellationToken ct)
    {
        var ids = partners.Select(p => p.Id).ToList();
        var venues = await db.Venues.AsNoTracking().Where(v => ids.Contains(v.PartnerId)).OrderBy(v => v.CreatedAt).ToListAsync(ct);
        var owners = await (from v in db.Venues
                            join m in db.VenueMembers on v.Id equals m.VenueId
                            join u in db.Users on m.UserId equals u.Id
                            where ids.Contains(v.PartnerId) && m.Role == "owner"
                            orderby u.CreatedAt
                            select new { v.PartnerId, u.Email, u.DisplayName }).AsNoTracking().ToListAsync(ct);
        return partners.Select(p =>
        {
            var owner = owners.FirstOrDefault(o => o.PartnerId == p.Id);
            return new PartnerDto(p.Id, p.LegalName, p.TaxId, p.BankName, p.BankAccount, p.CommissionRate, p.Status, p.CreatedAt,
                owner?.Email, owner?.DisplayName,
                venues.Where(v => v.PartnerId == p.Id).Select(v => new PartnerVenueDto(v.Id, v.Slug, v.NameTh, v.NameEn, v.Status)).ToList());
        }).ToList();
    }
}
