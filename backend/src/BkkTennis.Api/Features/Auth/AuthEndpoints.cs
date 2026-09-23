using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Domain;
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

public record OtpRequest(string Phone);
public record OtpVerify(string Phone, string Code, string? DisplayName);
public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
public record UserDto(Guid Id, string? Phone, string? DisplayName, string Role);

public static class AuthEndpoints
{
    private const int OtpTtlMinutes = 5;
    private const int MaxAttempts = 5;

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
        });

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
        });

        g.MapGet("/me", async (ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var id = principal.UserId();
            var user = await db.Users.FindAsync([id], ct);
            return user is null ? Results.NotFound() : Results.Ok(new UserDto(user.Id, user.Phone, user.DisplayName, user.Role));
        }).RequireAuthorization();

        return api;
    }

    public static AuthResponse Issue(User user, JwtOptions o, DateTime nowUtc)
    {
        var expires = nowUtc.AddDays(o.AccessTokenDays);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("phone", user.Phone ?? ""),
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(o.Issuer, o.Audience, claims, nowUtc, expires, creds);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            new UserDto(user.Id, user.Phone, user.DisplayName, user.Role));
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
