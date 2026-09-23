using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.Api.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace BkkTennis.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API against a per-run test database with a controllable clock.
/// Shared across test classes through <see cref="ApiCollection"/>; isolate tests with unique phone numbers / distinct slots.
/// The clock can only move forward, so tests that advance it must not depend on the absolute time afterwards.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private TestDatabase _db = null!;

    /// <summary>Starts at real "now" (DB defaults use now()); tests move it with <c>Advance</c>.</summary>
    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.UtcNow);

    /// <summary>Exceptions the API logged at Error level (i.e. behind a 500). Include them in assertion messages.</summary>
    public System.Collections.Concurrent.ConcurrentQueue<string> ServerErrors { get; } = new();

    public async Task InitializeAsync() => _db = await TestDatabase.CreateAsync();

    private sealed class ErrorCapture(System.Collections.Concurrent.ConcurrentQueue<string> sink) : Microsoft.Extensions.Logging.ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new Capture(sink, categoryName);
        public void Dispose() { }

        private sealed class Capture(System.Collections.Concurrent.ConcurrentQueue<string> sink, string category) : Microsoft.Extensions.Logging.ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            // EF logs every failed SaveChanges at Error even when the service handles it (e.g. 23P01 → 409); skip that noise.
            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level) =>
                level >= Microsoft.Extensions.Logging.LogLevel.Error && !category.StartsWith("Microsoft.EntityFrameworkCore");
            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel level, Microsoft.Extensions.Logging.EventId id, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                // Deliberate 4xx BookingExceptions are also logged by the exception-handler middleware; only genuine 500s matter.
                if (IsEnabled(level) && exception is not BkkTennis.Api.Features.Bookings.BookingException) sink.Enqueue($"[{category}] {formatter(state, exception)}\n{exception}");
            }
        }
    }

    private int _sqlCommands;

    /// <summary>Number of SQL commands EF has executed so far (all requests). Diff it around one request when nothing else runs.</summary>
    public int SqlCommands => Volatile.Read(ref _sqlCommands);

    private sealed class CommandCounter(ApiFactory owner) : Microsoft.Extensions.Logging.ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new Counter(owner);
        public void Dispose() { }

        private sealed class Counter(ApiFactory owner) : Microsoft.Extensions.Logging.ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level) => level >= Microsoft.Extensions.Logging.LogLevel.Information;
            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel level, Microsoft.Extensions.Logging.EventId id, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (id.Id == Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted.Id) Interlocked.Increment(ref owner._sqlCommands);
            }
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development: the OTP endpoint returns `devCode`, which the login helper relies on.
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", _db.ConnectionString);
        // Every test logs in through OTP from the same (null) client IP: keep the per-IP limiter out of the way
        // unless a test lowers it on purpose (see HardeningTests).
        builder.UseSetting("RateLimit:Otp:PermitLimit", "1000000");
        // Count executed SQL commands (EF logs them at Information; appsettings.json mutes that category to Warning).
        builder.ConfigureLogging(l => l.AddFilter<CommandCounter>("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.Information));
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider>(new CommandCounter(this));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider>(new ErrorCapture(ServerErrors));
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            // Tokens are minted with fake-clock timestamps, so validate their lifetime against the same clock —
            // otherwise advancing the clock would make freshly issued tokens "not yet valid" for the real-time validator.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
                o.TokenValidationParameters.LifetimeValidator = (notBefore, expires, _, p) =>
                {
                    var now = Clock.GetUtcNow().UtcDateTime;
                    return (notBefore is null || notBefore <= now + p.ClockSkew) && (expires is null || expires > now - p.ClockSkew);
                });
            // Tests drive expiry explicitly via BookingService.ExpireHoldsAsync instead of a 30 s timer.
            foreach (var d in services.Where(d => d.ImplementationType == typeof(HoldExpiryJob)).ToList())
                services.Remove(d);
        });
    }

    /// <summary>Runs <paramref name="work"/> in a fresh DI scope (DbContext, BookingService, ...).</summary>
    public async Task<T> WithScope<T>(Func<IServiceProvider, Task<T>> work)
    {
        using var scope = Services.CreateScope();
        return await work(scope.ServiceProvider);
    }

    public Task WithDb(Func<AppDbContext, Task> work) =>
        WithScope<int>(async sp =>
        {
            await work(sp.GetRequiredService<AppDbContext>());
            return 0;
        });

    private static int _phoneSeq = Random.Shared.Next(10_000_000, 90_000_000);

    /// <summary>Unique Thai mobile number (08xxxxxxxx) so tests never share OTP rate limits or users.</summary>
    public static string NewPhone() => "08" + Interlocked.Increment(ref _phoneSeq).ToString("D8")[^8..];

    public HttpClient Anonymous() => CreateClient();

    /// <summary>
    /// A fresh partner user (OTP login, then promoted in the DB and re-logged-in so the token carries the new role).
    /// Pass <paramref name="venueId"/> to make them a member of that venue with <paramref name="memberRole"/> ("staff" | "owner").
    /// </summary>
    public async Task<TestUser> LoginPartnerAsync(Guid? venueId, string memberRole = "staff", string role = "partner_staff")
    {
        var first = await LoginAsync();
        await WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE users SET role = {role} WHERE id = {first.Id}");
            if (venueId is { } v)
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO venue_members (venue_id, user_id, role) VALUES ({v}, {first.Id}, {memberRole})");
        });
        return await LoginAsync(first.Phone);
    }

    /// <summary>Logs in via the real OTP flow and returns a client carrying the bearer token.</summary>
    public async Task<TestUser> LoginAsync(string? phone = null, string? displayName = null)
    {
        phone ??= NewPhone();
        var client = CreateClient();

        var req = await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone });
        req.EnsureSuccessStatusCode();
        var devCode = (await req.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("devCode").GetString()!;

        var verify = await client.PostAsJsonAsync("/api/v1/auth/otp/verify", new { phone, code = devCode, displayName });
        verify.EnsureSuccessStatusCode();
        var body = await verify.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.GetProperty("accessToken").GetString());
        return new TestUser(client, body.GetProperty("user").GetProperty("id").GetGuid(), phone);
    }
}

public sealed record TestUser(HttpClient Http, Guid Id, string Phone);

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
