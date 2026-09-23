using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.Api.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    public async Task InitializeAsync() => _db = await TestDatabase.CreateAsync();

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
        builder.ConfigureServices(services =>
        {
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
