using System.Globalization;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Admin;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Features.ClubOs;
using BkkTennis.Api.Features.Payments;
using BkkTennis.Api.Features.Venues;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Jobs;
using BkkTennis.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Wire formats (yyyy-MM-dd, HH:mm) must not depend on the host locale: a th-TH machine would emit Buddhist-era years.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// ---------------------------------------------------------------- options
builder.Services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
builder.Services.Configure<PaymentOptions>(cfg.GetSection("Payments"));
builder.Services.Configure<BookingOptions>(cfg.GetSection("Booking"));
builder.Services.Configure<RateLimitOptions>(cfg.GetSection("RateLimit"));
var jwt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

// ---------------------------------------------------------------- data
builder.Services.AddDbContext<AppDbContext>(o => o
    .UseNpgsql(cfg.GetConnectionString("Default"))
    .UseSnakeCaseNamingConvention());

// ---------------------------------------------------------------- services
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddSingleton<AvailabilityNotifier>();
builder.Services.AddSingleton<IPaymentProvider, MockPaymentProvider>(); // TODO(sprint 4): real gateway by Payments:Provider
builder.Services.AddScoped<PayoutService>();
builder.Services.AddHostedService<HoldExpiryJob>();
builder.Services.AddHostedService<PayoutJob>();
builder.Services.AddSignalR();

// ---------------------------------------------------------------- auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        // SignalR sends the token as ?access_token=
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddVenueAuthorization();

// ---------------------------------------------------------------- http
var origins = cfg.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.') ?? t.Name));
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddApiRateLimiting();

var app = builder.Build();

// CORS first so error responses (500 etc.) still carry CORS headers and the browser can read them.
app.UseCors();

// Expected failures (BookingException, malformed requests) are answered by ApiExceptionHandler; anything else is a 500.
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    var (code, message) = ApiErrors.For(StatusCodes.Status500InternalServerError);
    await ctx.Response.WriteAsJsonAsync(new { code, message });
}));
// Bodiless 401/403/404/405 … get the same { code, message } JSON.
app.UseStatusCodePages(ApiErrors.WriteStatusPageAsync);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Liveness + database reachability (503 when Postgres is down, so orchestrators stop routing to this instance).
app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
{
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
    timeout.CancelAfter(TimeSpan.FromSeconds(3));
    var up = false;
    try { up = await db.Database.CanConnectAsync(timeout.Token); }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
    return up
        ? Results.Ok(new { status = "ok", db = "up" })
        : Results.Json(new { status = "degraded", db = "down" }, statusCode: StatusCodes.Status503ServiceUnavailable);
});

var api = app.MapGroup("/api/v1");
api.MapAuth();
api.MapVenues();
api.MapFavorites();
api.MapBookings();
api.MapClubOs();
api.MapAdmin();
api.MapPayments(app.Environment);

app.MapHub<AvailabilityHub>("/hubs/availability");

app.Run();

public partial class Program;
