using System.Globalization;
using System.Threading.RateLimiting;
using BkkTennis.Api.Features.Bookings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BkkTennis.Api.Services;

/// <summary>Every error the API returns is JSON <c>{ code, message }</c> (validation adds <c>errors</c>). Codes are stable, messages are Thai.</summary>
public static class ApiErrors
{
    public static (string Code, string Message) For(int status) => status switch
    {
        StatusCodes.Status400BadRequest => ("bad_request", "คำขอไม่ถูกต้อง"),
        StatusCodes.Status401Unauthorized => ("unauthorized", "กรุณาเข้าสู่ระบบ"),
        StatusCodes.Status403Forbidden => ("forbidden", "ไม่มีสิทธิ์เข้าถึง"),
        StatusCodes.Status404NotFound => ("not_found", "ไม่พบข้อมูล"),
        StatusCodes.Status405MethodNotAllowed => ("method_not_allowed", "ไม่รองรับวิธีการเรียกนี้"),
        StatusCodes.Status409Conflict => ("conflict", "ทำรายการไม่ได้ในสถานะปัจจุบัน"),
        StatusCodes.Status415UnsupportedMediaType => ("unsupported_media_type", "ต้องส่งข้อมูลเป็น application/json"),
        StatusCodes.Status429TooManyRequests => ("rate_limited", "ส่งคำขอบ่อยเกินไป กรุณาลองใหม่ภายหลัง"),
        >= 500 => ("server_error", "เกิดข้อผิดพลาด กรุณาลองใหม่"),
        _ => ("error", "เกิดข้อผิดพลาด"),
    };

    /// <summary>Fills in the JSON body for bodiless 4xx/5xx responses (Results.NotFound(), auth challenges, 405 …).</summary>
    public static Task WriteStatusPageAsync(StatusCodeContext ctx)
    {
        var (code, message) = For(ctx.HttpContext.Response.StatusCode);
        return ctx.HttpContext.Response.WriteAsJsonAsync(new { code, message });
    }

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // OTP endpoints send SMS and are the brute-force target: limit per client IP (in addition to the per-phone limits).
            // Options are read per request so deployments/tests can override them through configuration.
            o.AddPolicy("otp", http =>
            {
                var opt = http.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value.Otp;
                return RateLimitPartition.GetFixedWindowLimiter(http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = opt.PermitLimit,
                        Window = TimeSpan.FromSeconds(opt.WindowSeconds),
                        QueueLimit = 0,
                    });
            });
            o.OnRejected = async (ctx, ct) =>
            {
                var window = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value.Otp.WindowSeconds;
                var retryAfter = ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra) ? (int)Math.Ceiling(ra.TotalSeconds) : window;
                ctx.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                var (code, message) = For(StatusCodes.Status429TooManyRequests);
                await ctx.HttpContext.Response.WriteAsJsonAsync(new { code, message }, ct);
            };
        });
        return services;
    }
}

public class RateLimitOptions
{
    public LimitOptions Otp { get; set; } = new();

    public class LimitOptions
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }
}

/// <summary>Turns expected failures (domain rules, malformed requests) into the standard error body; anything else falls through to the 500 handler.</summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        int status;
        string code, message;
        switch (ex)
        {
            case BookingException be:
                (status, code, message) = (be.Status, be.Code, be.Message);
                break;
            case BadHttpRequestException bad:
                status = bad.StatusCode;
                (code, message) = For(status);
                break;
            default:
                return false;
        }

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new { code, message }, ct);
        return true;
    }

    private static (string, string) For(int status) =>
        status == StatusCodes.Status415UnsupportedMediaType ? ApiErrors.For(status) : ApiErrors.For(StatusCodes.Status400BadRequest);
}
