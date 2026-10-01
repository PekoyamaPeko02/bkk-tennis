using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BkkTennis.IntegrationTests;

/// <summary>T6: health check, OTP rate limiting, request validation and one consistent {code,message} error shape.</summary>
[Collection(ApiCollection.Name)]
public class HardeningTests(ApiFactory api)
{
    private static readonly Guid Venue = Seed.VenueId;

    /// <summary>Every 4xx/5xx the API produces must be JSON {code, message} (validation adds `errors`).</summary>
    private static async Task<JsonElement> AssertErrorAsync(HttpResponseMessage res, HttpStatusCode status, string code)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} but got {(int)res.StatusCode}: {raw}");
        Assert.Contains("json", res.Content.Headers.ContentType?.MediaType ?? "");
        var body = JsonDocument.Parse(raw).RootElement;
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()), "message must not be empty: " + raw);
        return body;
    }

    private static StringContent Json(string raw, string mediaType = "application/json") =>
        new(raw, Encoding.UTF8, mediaType);

    // ------------------------------------------------------------------ /health

    [Fact]
    public async Task Health_reports_ok_and_that_the_database_is_reachable()
    {
        var res = await api.Anonymous().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ok", body.GetProperty("status").GetString());
        Assert.Equal("up", body.GetProperty("db").GetString());
    }

    [Fact]
    public async Task Health_is_503_when_the_database_is_unreachable()
    {
        using var broken = api.WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:Default", "Host=127.0.0.1;Port=1;Database=nope;Username=x;Password=x;Timeout=2;Command Timeout=2"));

        var res = await broken.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("degraded", body.GetProperty("status").GetString());
        Assert.Equal("down", body.GetProperty("db").GetString());
    }

    // ------------------------------------------------------------------ rate limiting

    private WebApplicationFactory<Program> WithOtpLimit(int permits, int windowSeconds) => api.WithWebHostBuilder(b =>
    {
        b.UseSetting("RateLimit:Otp:PermitLimit", permits.ToString());
        b.UseSetting("RateLimit:Otp:WindowSeconds", windowSeconds.ToString());
    });

    [Fact]
    public async Task Otp_endpoints_are_rate_limited_per_client_with_a_json_429_and_retry_after()
    {
        using var limited = WithOtpLimit(permits: 3, windowSeconds: 30);
        var client = limited.CreateClient();
        var phone = ApiFactory.NewPhone();

        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone })).StatusCode);

        var blocked = await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = ApiFactory.NewPhone() });
        await AssertErrorAsync(blocked, HttpStatusCode.TooManyRequests, "rate_limited");
        Assert.True(blocked.Headers.RetryAfter is not null, "429 must carry Retry-After");

        // verify shares the budget (it is the brute-force target)…
        var verify = await client.PostAsJsonAsync("/api/v1/auth/otp/verify", new { phone, code = "123456" });
        await AssertErrorAsync(verify, HttpStatusCode.TooManyRequests, "rate_limited");

        // …but the rest of the API is untouched
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/venues")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Otp_rate_limit_recovers_after_the_window()
    {
        using var limited = WithOtpLimit(permits: 2, windowSeconds: 1);
        var client = limited.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = ApiFactory.NewPhone() })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = ApiFactory.NewPhone() })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = ApiFactory.NewPhone() })).StatusCode);

        await Task.Delay(TimeSpan.FromMilliseconds(1300)); // real time: the limiter's window is not driven by the fake clock

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = ApiFactory.NewPhone() })).StatusCode);
    }

    // ------------------------------------------------------------------ malformed input → 400 {code,message}

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("[1,2,3]")]
    public async Task Malformed_or_missing_body_is_400_bad_request(string raw)
    {
        var res = await api.Anonymous().PostAsync("/api/v1/auth/otp/request", Json(raw));

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, "bad_request");
    }

    [Fact]
    public async Task Non_json_content_type_is_415()
    {
        var res = await api.Anonymous().PostAsync("/api/v1/auth/otp/request", Json("phone=0812345678", "text/plain"));

        await AssertErrorAsync(res, HttpStatusCode.UnsupportedMediaType, "unsupported_media_type");
    }

    [Fact]
    public async Task Bad_phone_keeps_its_specific_code()
    {
        var res = await api.Anonymous().PostAsJsonAsync("/api/v1/auth/otp/request", new { phone = "12345" });

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, "bad_phone");
    }

    // ------------------------------------------------------------------ DTO validation

    [Theory]
    [InlineData("""{ }""", "phone")]
    [InlineData("""{ "phone": null }""", "phone")]
    [InlineData("""{ "phone": "" }""", "phone")]
    public async Task Otp_request_requires_a_phone(string raw, string field)
    {
        var res = await api.Anonymous().PostAsync("/api/v1/auth/otp/request", Json(raw));

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty(field, out var messages) && messages.GetArrayLength() > 0,
            "errors must be keyed by field: " + body);
    }

    [Fact]
    public async Task Otp_verify_without_a_code_is_400_not_500()
    {
        var res = await api.Anonymous().PostAsJsonAsync("/api/v1/auth/otp/verify", new { phone = ApiFactory.NewPhone() });

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("code", out _), body.ToString());
    }

    [Fact]
    public async Task Overlong_fields_are_rejected()
    {
        var res = await api.Anonymous().PostAsJsonAsync("/api/v1/auth/otp/verify",
            new { phone = ApiFactory.NewPhone(), code = "123456", displayName = new string('x', 500) });

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("displayName", out _), body.ToString());
    }

    [Fact]
    public async Task Booking_request_is_validated_including_nested_slots_and_addons()
    {
        var user = await api.LoginAsync();
        var date = DateOnly.FromDateTime(api.Clock.GetUtcNow().UtcDateTime).AddDays(3).ToString("yyyy-MM-dd");

        var empties = await user.Http.PostAsync("/api/v1/bookings", Json(
            $$"""{ "venueId": "{{Guid.Empty}}", "date": "{{date}}", "slots": [ { "courtId": "{{Guid.Empty}}", "start": null } ], "addons": [ { "addonId": "{{Guid.Empty}}", "qty": 9999 } ] }"""));

        var body = await AssertErrorAsync(empties, HttpStatusCode.BadRequest, "validation_failed");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("venueId", out _), body.ToString());
        Assert.True(errors.TryGetProperty("slots[0].courtId", out _), body.ToString());
        Assert.True(errors.TryGetProperty("slots[0].start", out _), body.ToString());
        Assert.True(errors.TryGetProperty("addons[0].addonId", out _), body.ToString());
        Assert.True(errors.TryGetProperty("addons[0].qty", out _), body.ToString());
    }

    [Fact]
    public async Task Missing_slots_keep_the_domain_error_code()
    {
        var user = await api.LoginAsync();
        var date = DateOnly.FromDateTime(api.Clock.GetUtcNow().UtcDateTime).AddDays(3).ToString("yyyy-MM-dd");

        var res = await user.Http.PostAsync("/api/v1/bookings", Json($$"""{ "venueId": "{{Venue}}", "date": "{{date}}", "slots": [] }"""));

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, "no_slots");
    }

    [Fact]
    public async Task Valid_requests_still_pass_validation()
    {
        var user = await api.LoginAsync();

        var res = await user.Http.HoldAsync(SlotPicker.Next(api));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    // ------------------------------------------------------------------ one error shape everywhere

    [Fact]
    public async Task Unknown_route_is_404_not_found()
    {
        await AssertErrorAsync(await api.Anonymous().GetAsync("/api/v1/nope"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Unknown_venue_and_availability_are_404_not_found()
    {
        await AssertErrorAsync(await api.Anonymous().GetAsync("/api/v1/venues/no-such-venue"), HttpStatusCode.NotFound, "not_found");
        await AssertErrorAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{Guid.NewGuid()}/availability"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Someone_elses_booking_is_404_not_found()
    {
        var alice = await api.LoginAsync();
        var mallory = await api.LoginAsync();
        var held = await alice.Http.HoldOkAsync(SlotPicker.Next(api));

        await AssertErrorAsync(await mallory.Http.GetAsync($"/api/v1/me/bookings/{held.Id()}"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Missing_or_invalid_token_is_401_unauthorized()
    {
        await AssertErrorAsync(await api.Anonymous().GetAsync("/api/v1/me/bookings"), HttpStatusCode.Unauthorized, "unauthorized");

        var bad = api.Anonymous();
        bad.DefaultRequestHeaders.Authorization = new("Bearer", "not.a.jwt");
        await AssertErrorAsync(await bad.GetAsync("/api/v1/auth/me"), HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Fact]
    public async Task A_player_on_a_partner_endpoint_is_403_forbidden()
    {
        var player = await api.LoginAsync();

        var res = await player.Http.GetAsync($"/api/v1/clubos/venues/{Venue}/bookings?date=2030-01-01");

        await AssertErrorAsync(res, HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task Wrong_method_is_405_method_not_allowed()
    {
        await AssertErrorAsync(await api.Anonymous().GetAsync("/api/v1/auth/otp/request"), HttpStatusCode.MethodNotAllowed, "method_not_allowed");
    }

    [Fact]
    public async Task Domain_conflicts_keep_their_code_and_message()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        Assert.Equal(HttpStatusCode.OK, (await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);

        var again = await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);

        await AssertErrorAsync(again, HttpStatusCode.Conflict, "not_cancellable");
    }
}
