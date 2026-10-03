using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
using BkkTennis.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BkkTennis.Api.Features.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "bkk-tennis";
    public string Audience { get; set; } = "bkk-tennis-apps";
    /// <summary>At least 32 chars. Override via env Jwt__SigningKey in every non-dev environment.</summary>
    public string SigningKey { get; set; } = "dev-only-signing-key-change-me-0123456789";
    public int AccessTokenDays { get; set; } = 7;
}

public record OtpRequest([property: Required, MaxLength(20)] string Phone);
public record OtpVerify(
    [property: Required, MaxLength(20)] string Phone,
    [property: Required, MaxLength(12)] string Code,
    [property: MaxLength(80)] string? DisplayName);
public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
public record UserDto(Guid Id, string? Phone, string? DisplayName, string Role, string? Email = null);
public record StaffLogin([property: Required, MaxLength(254)] string Email, [property: Required, MaxLength(128), MaxUtf8Bytes(72)] string Password);

public static class AuthEndpoints
{
    private const int OtpTtlMinutes = 5;
    private const int MaxAttempts = 5;
    private const int MaxFailedLogins = 5;
    private const int LockMinutes = 15;
    /// <summary>Verified when the account is unknown/ineligible so both paths cost the same bcrypt work.</summary>
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), workFactor: 10);

    public static RouteGroupBuilder MapAuth(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/otp/request", async (OtpRequest req, AppDbContext db, IWebHostEnvironment env,
            ILoggerFactory lf, TimeProvider clock, CancellationToken ct) =>
        {
            var phone = NormalizePhone(req.Phone);
            if (phone is null) return Results.BadRequest(new { code = "bad_phone", message = "เบอร์โทรไม่ถูกต้อง" });

            var now = clock.GetUtcNow().UtcDateTime;
            var recent = await db.OtpCodes.CountAsync(o => o.Phone == phone && o.CreatedAt > now.AddMinutes(-10), ct);
            if (recent >= 3) return Results.Json(new { code = "rate_limited", message = "ขอรหัสบ่อยเกินไป ลองใหม่ภายหลัง" }, statusCode: 429);

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            db.OtpCodes.Add(new OtpCode { Id = Guid.NewGuid(), Phone = phone, CodeHash = Hash(phone, code), ExpiresAt = now.AddMinutes(OtpTtlMinutes) });
            await db.SaveChangesAsync(ct);

            // TODO(sprint 4): send via SMS provider. In Development the code is returned for convenience.
            lf.CreateLogger("Otp").LogInformation("OTP for {Phone}: {Code}", phone, env.IsDevelopment() ? code : "******");
            return Results.Ok(new { phone, expiresInSeconds = OtpTtlMinutes * 60, devCode = env.IsDevelopment() ? code : null });
        }).RequireRateLimiting("otp").WithValidation();

        g.MapPost("/otp/verify", async (OtpVerify req, AppDbContext db, IOptions<JwtOptions> jwt,
            TimeProvider clock, CancellationToken ct) =>
        {
            var phone = NormalizePhone(req.Phone);
            if (phone is null) return Results.BadRequest(new { code = "bad_phone", message = "เบอร์โทรไม่ถูกต้อง" });
            var now = clock.GetUtcNow().UtcDateTime;

            var otp = await db.OtpCodes
                .Where(o => o.Phone == phone && o.ConsumedAt == null && o.ExpiresAt > now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (otp is null || otp.Attempts >= MaxAttempts)
                return Results.BadRequest(new { code = "otp_expired", message = "รหัสหมดอายุ กรุณาขอรหัสใหม่" });

            otp.Attempts++;
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(otp.CodeHash), Encoding.UTF8.GetBytes(Hash(phone, req.Code.Trim()))))
            {
                await db.SaveChangesAsync(ct);
                return Results.BadRequest(new { code = "otp_invalid", message = "รหัสไม่ถูกต้อง" });
            }
            otp.ConsumedAt = now;

            var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == phone, ct);
            if (user is null)
            {
                user = new User { Id = Guid.NewGuid(), Phone = phone, DisplayName = req.DisplayName, Role = Roles.Player };
                db.Users.Add(user);
            }
            await db.SaveChangesAsync(ct);

            return Results.Ok(Issue(user, jwt.Value, now));
        }).RequireRateLimiting("otp").WithValidation();

        // Partner staff (ClubOS): email + password. Same JWT as the OTP flow; venue access is decided per venue by VenueAuthorization.
        g.MapPost("/staff/login", (StaffLogin req, AppDbContext db, IOptions<JwtOptions> jwt, TimeProvider clock, CancellationToken ct) =>
            PasswordLoginAsync(req, [Roles.PartnerStaff, Roles.PartnerOwner], db, jwt.Value, clock, ct))
            .RequireRateLimiting("otp").WithValidation();

        // Platform admins (Admin portal): same mechanics and lockout, but only platform_admin accounts; partner accounts can't use it.
        g.MapPost("/admin/login", (StaffLogin req, AppDbContext db, IOptions<JwtOptions> jwt, TimeProvider clock, CancellationToken ct) =>
            PasswordLoginAsync(req, [Roles.PlatformAdmin], db, jwt.Value, clock, ct))
            .RequireRateLimiting("otp").WithValidation();

        g.MapGet("/me", async (ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var id = principal.UserId();
            var user = await db.Users.FindAsync([id], ct);
            return user is null ? Results.NotFound() : Results.Ok(new UserDto(user.Id, user.Phone, user.DisplayName, user.Role, user.Email));
        }).RequireAuthorization();

        g.MapPasswords();

        return api;
    }

    private static async Task<IResult> PasswordLoginAsync(
        StaffLogin req, string[] allowedRoles, AppDbContext db, JwtOptions jwt, TimeProvider clock, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var now = clock.GetUtcNow().UtcDateTime;
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        var eligible = user is { PasswordHash: not null } && allowedRoles.Contains(user.Role);
        var locked = user?.LockedUntil > now;
        // Always run one bcrypt verify (against a dummy hash if need be) so timing doesn't reveal whether the email exists.
        var passwordOk = BCrypt.Net.BCrypt.Verify(req.Password, eligible ? user!.PasswordHash! : DummyHash);

        if (!eligible || locked || !passwordOk)
        {
            if (eligible && !locked) await RegisterFailedPasswordAsync(db, user!.Id, now, ct);
            // 400, not 401: the client signs the user out on any 401 (expired session), which would loop on the login form.
            return Results.BadRequest(new { code = "invalid_credentials", message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง (หรือบัญชีถูกล็อกชั่วคราว)" });
        }

        if (user!.FailedLogins != 0 || user.LockedUntil is not null)
        {
            user.FailedLogins = 0;
            user.LockedUntil = null;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(Issue(user, jwt, now));
    }

    /// <summary>Atomic, so parallel guesses can't slip past the limit: the 5th consecutive failure locks the account.</summary>
    internal static Task RegisterFailedPasswordAsync(AppDbContext db, Guid userId, DateTime now, CancellationToken ct)
    {
        var lockUntil = now.AddMinutes(LockMinutes);
        return db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s
            .SetProperty(u => u.LockedUntil, u => u.FailedLogins + 1 >= MaxFailedLogins ? lockUntil : u.LockedUntil)
            .SetProperty(u => u.FailedLogins, u => u.FailedLogins + 1 >= MaxFailedLogins ? 0 : u.FailedLogins + 1), ct);
    }

    /// <summary>
    /// Changes with every password change/reset (unix ms of <c>password_changed_at</c>, "0" if never). It rides in the JWT so a new
    /// password logs out every older session; tokens without the claim count as "0".
    /// </summary>
    public static string Stamp(DateTime? passwordChangedAt) =>
        passwordChangedAt is { } t ? new DateTimeOffset(DateTime.SpecifyKind(t, DateTimeKind.Utc)).ToUnixTimeMilliseconds().ToString() : "0";

    public const string StampClaim = "pwd";

    public static AuthResponse Issue(User user, JwtOptions o, DateTime nowUtc)
    {
        var expires = nowUtc.AddDays(o.AccessTokenDays);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("phone", user.Phone ?? ""),
            new Claim(StampClaim, Stamp(user.PasswordChangedAt)),
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(o.Issuer, o.Audience, claims, nowUtc, expires, creds);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            new UserDto(user.Id, user.Phone, user.DisplayName, user.Role, user.Email));
    }

    /// <summary>Accepts 0812345678 / +66812345678 / 66812345678 → +66812345678.</summary>
    public static string? NormalizePhone(string? raw)
    {
        var d = new string((raw ?? "").Where(char.IsDigit).ToArray());
        if (d.StartsWith("66")) d = d[2..];
        d = d.TrimStart('0');
        return d.Length == 9 ? "+66" + d : null;
    }

    private static string Hash(string phone, string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{phone}:{code}")));

    public static Guid UserId(this ClaimsPrincipal p) =>
        Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier) ?? p.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? throw new UnauthorizedAccessException());
}
