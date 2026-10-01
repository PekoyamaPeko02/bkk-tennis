using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BkkTennis.Api.Features.Auth;

public record ChangePasswordRequest(
    [property: Required, MaxLength(128), MaxUtf8Bytes(72)] string CurrentPassword,
    [property: Required, MinLength(10), MaxLength(128), MaxUtf8Bytes(72)] string NewPassword);
public record ResetPasswordRequest(
    [property: Required, MaxLength(100)] string Token,
    [property: Required, MinLength(10), MaxLength(128), MaxUtf8Bytes(72)] string NewPassword);

public class PasswordResetOptions
{
    public int TokenHours { get; set; } = 24;
    /// <summary>Where ClubOS lives; the admin gets a ready-to-copy link <c>{ClubOsUrl}/reset-password?token=…</c>.</summary>
    public string ClubOsUrl { get; set; } = "http://localhost:9001";
}

/// <summary>Partner password change (signed in) and reset (admin-issued one-time link). Every change logs out older sessions.</summary>
public static class PasswordEndpoints
{
    private static readonly string[] PartnerRoles = [Roles.PartnerStaff, Roles.PartnerOwner];

    public static void MapPasswords(this RouteGroupBuilder g)
    {
        // The current password is re-checked (and wrong guesses count towards the login lockout) so a stolen session can't take the account over.
        g.MapPost("/password/change", async (ChangePasswordRequest req, ClaimsPrincipal principal, AppDbContext db,
            IOptions<JwtOptions> jwt, TimeProvider clock, CancellationToken ct) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == principal.UserId(), ct);
            if (user?.PasswordHash is null)
                throw new BookingException(400, "no_password", "บัญชีนี้ไม่ได้ใช้รหัสผ่าน");
            var now = clock.GetUtcNow().UtcDateTime;
            var locked = user.LockedUntil > now;
            var ok = !locked && BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash);
            if (!ok)
            {
                if (!locked) await AuthEndpoints.RegisterFailedPasswordAsync(db, user.Id, now, ct);
                throw new BookingException(400, "wrong_password", "รหัสผ่านปัจจุบันไม่ถูกต้อง");
            }
            if (req.NewPassword == req.CurrentPassword)
                throw new BookingException(400, "same_password", "รหัสผ่านใหม่ต้องต่างจากรหัสผ่านเดิม");

            await SetPasswordAsync(db, user, req.NewPassword, now, ct);
            // The old token just died with the old stamp; hand back a fresh one so the caller stays signed in.
            return Results.Ok(AuthEndpoints.Issue(user, jwt.Value, now));
        }).RequireAuthorization().RequireRateLimiting("otp").WithValidation();

        // Anonymous: the token is the credential. Unknown, expired and used tokens all look the same.
        g.MapPost("/password/reset", async (ResetPasswordRequest req, AppDbContext db, TimeProvider clock, CancellationToken ct) =>
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var hash = HashToken(req.Token.Trim());
            var row = await db.PasswordResetTokens.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == hash && t.ConsumedAt == null && t.ExpiresAt > now, ct);
            var user = row is null ? null : await db.Users.FirstOrDefaultAsync(u => u.Id == row.UserId, ct);
            if (row is null || user is null || !PartnerRoles.Contains(user.Role))
                throw new BookingException(400, "reset_token_invalid", "ลิงก์รีเซ็ตรหัสผ่านไม่ถูกต้องหรือหมดอายุแล้ว");

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            // Consume first and atomically: of two concurrent uses only one sees a row to update.
            var consumed = await db.PasswordResetTokens.Where(t => t.Id == row.Id && t.ConsumedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now), ct);
            if (consumed == 0)
                throw new BookingException(400, "reset_token_invalid", "ลิงก์รีเซ็ตรหัสผ่านไม่ถูกต้องหรือหมดอายุแล้ว");
            await SetPasswordAsync(db, user, req.NewPassword, now, ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        }).RequireRateLimiting("otp").WithValidation();
    }

    private static async Task SetPasswordAsync(AppDbContext db, User user, string password, DateTime now, CancellationToken ct)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        user.PasswordChangedAt = now;
        user.FailedLogins = 0;
        user.LockedUntil = null;
        await db.SaveChangesAsync(ct);
        // Any other outstanding reset link is void now.
        await db.PasswordResetTokens.Where(t => t.UserId == user.Id && t.ConsumedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now), ct);
    }

    /// <summary>Creates a fresh single-use token for the user (voiding older ones). Returns the plaintext once; only its hash is kept.</summary>
    public static async Task<(string Token, DateTime ExpiresAt)> IssueResetTokenAsync(
        AppDbContext db, Guid userId, Guid? createdBy, PasswordResetOptions opts, DateTime now, CancellationToken ct)
    {
        await db.PasswordResetTokens.Where(t => t.UserId == userId && t.ConsumedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now), ct);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expires = now.AddHours(opts.TokenHours);
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(), UserId = userId, TokenHash = HashToken(token), ExpiresAt = expires, CreatedBy = createdBy,
        });
        await db.SaveChangesAsync(ct);
        return (token, expires);
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
