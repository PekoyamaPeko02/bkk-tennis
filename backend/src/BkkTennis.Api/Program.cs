using System.Globalization;
using System.Text;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Features.ClubOs;
using BkkTennis.Api.Features.Payments;
using BkkTennis.Api.Features.Venues;
using BkkTennis.Api.Hubs;
using BkkTennis.Api.Jobs;
using BkkTennis.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
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
var jwt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

// ---------------------------------------------------------------- data
builder.Services.AddDbContext<AppDbContext>(o => o
    .UseNpgsql(cfg.GetConnectionString("Default"))
    .UseSnakeCaseNamingConvention());

// ---------------------------------------------------------------- services
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddSingleton<AvailabilityNotifier>();
builder.Services.AddSingleton<IPaymentProvider, MockPaymentProvider>(); // TODO(sprint 4): real gateway by Payments:Provider
builder.Services.AddHostedService<HoldExpiryJob>();
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
builder.Services.AddAuthorization();

// ---------------------------------------------------------------- http
var origins = cfg.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.') ?? t.Name));
builder.Services.AddProblemDetails();

var app = builder.Build();

// CORS first so error responses (500 etc.) still carry CORS headers and the browser can read them.
app.UseCors();

app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ex is BookingException be)
    {
        ctx.Response.StatusCode = be.Status;
        await ctx.Response.WriteAsJsonAsync(new { code = be.Code, message = be.Message });
        return;
    }
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await ctx.Response.WriteAsJsonAsync(new { code = "server_error", message = "เกิดข้อผิดพลาด กรุณาลองใหม่" });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var api = app.MapGroup("/api/v1");
api.MapAuth();
api.MapVenues();
api.MapBookings();
api.MapClubOs();
api.MapPayments(app.Environment);

app.MapHub<AvailabilityHub>("/hubs/availability");

app.Run();

public partial class Program;
