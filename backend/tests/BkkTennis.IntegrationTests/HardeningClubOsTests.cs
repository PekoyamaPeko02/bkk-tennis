using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Features.ClubOs;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using BkkTennis.Api.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// T6 regression net for the ClubOS endpoints: the new DTO validation / error plumbing must not change their domain
/// error codes, and validation must not run before the venue-membership / owner authorization filters.
/// </summary>
[Collection(ApiCollection.Name)]
public class HardeningClubOsTests(ApiFactory api)
{
    private static readonly Guid Venue = Seed.VenueId;
    private const string OwnerPhone = "0800000002"; // seeded partner_owner of the seed venue
    private static string Base => $"/api/v1/clubos/venues/{Venue}";

    private static StringContent Json(string raw) => new(raw, Encoding.UTF8, "application/json");

    private static async Task<JsonElement> AssertErrorAsync(HttpResponseMessage res, HttpStatusCode status, string code)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} but got {(int)res.StatusCode}: {raw}");
        var body = JsonDocument.Parse(raw).RootElement;
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()), raw);
        return body;
    }

    // The per-phone OTP limit (3 per 10 min) would 429 a login per test: log the seeded owner in once per test run.
    private static readonly SemaphoreSlim OwnerLock = new(1, 1);
    private static TestUser? _owner;

    private async Task<TestUser> OwnerAsync()
    {
        await OwnerLock.WaitAsync();
        try { return _owner ??= await api.LoginAsync(OwnerPhone); }
        finally { OwnerLock.Release(); }
    }

    /// <summary>A user with the partner_staff role; a member of the seed venue only when <paramref name="member"/>.</summary>
    private async Task<TestUser> StaffAsync(bool member)
    {
        var first = await api.LoginAsync();
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE users SET role = 'partner_staff' WHERE id = {first.Id}");
            if (member)
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO venue_members (venue_id, user_id, role) VALUES ({Venue}, {first.Id}, 'staff')");
        });
        return await api.LoginAsync(first.Phone); // new token carries the new role
    }

    private static string Utc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

    private static string BlockBody(Guid courtId, DateTime startUtc, DateTime endUtc, string? reason = null) =>
        JsonSerializer.Serialize(new { courtId, startAt = Utc(startUtc), endAt = Utc(endUtc), reason });

    private static string PriceBody(string dayType = "weekday", string start = "03:00", string end = "04:00",
        decimal price = 100, int priority = 5, Guid? courtId = null) =>
        JsonSerializer.Serialize(new { courtId, dayType, start, end, pricePerHour = price, priority });

    private static string WalkInBody(SlotBlock b, string? start = null, string? customerName = null) =>
        JsonSerializer.Serialize(new
        {
            date = b.Date.ToString("yyyy-MM-dd"),
            slots = new[] { new { courtId = b.CourtId, start = start ?? b.First } },
            customerName,
        });

    // ------------------------------------------------------------------ court blocks

    [Theory]
    [InlineData("party", "bad_reason")]
    [InlineData("MAINTENANCE", "bad_reason")]
    public async Task Block_with_an_unknown_reason_keeps_bad_reason(string reason, string code)
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var res = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(b.CourtId, b.FirstStartUtc, b.FirstStartUtc.AddHours(1), reason)));

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, code);
    }

    [Fact]
    public async Task Block_ending_before_it_starts_keeps_bad_range()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var res = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(b.CourtId, b.FirstStartUtc, b.FirstStartUtc.AddHours(-1))));

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, "bad_range");
    }

    [Fact]
    public async Task Block_on_an_unknown_court_is_404_court_not_found()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var res = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(Guid.NewGuid(), b.FirstStartUtc, b.FirstStartUtc.AddHours(1))));

        await AssertErrorAsync(res, HttpStatusCode.NotFound, "court_not_found");
    }

    [Fact]
    public async Task Block_over_a_live_booking_keeps_409_block_conflict()
    {
        var player = await api.LoginAsync();
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);
        await player.Http.HoldOkAsync(b);

        var res = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(b.CourtId, b.FirstStartUtc, b.FirstStartUtc.AddHours(1))));

        await AssertErrorAsync(res, HttpStatusCode.Conflict, "block_conflict");
    }

    [Fact]
    public async Task Block_request_is_validated_by_dto_rules()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var emptyCourt = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(Guid.Empty, b.FirstStartUtc, b.FirstStartUtc.AddHours(1))));
        var body = await AssertErrorAsync(emptyCourt, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("courtId", out _), body.ToString());

        var longNote = await owner.Http.PostAsync($"{Base}/blocks", Json(JsonSerializer.Serialize(new
        {
            courtId = b.CourtId, startAt = Utc(b.FirstStartUtc), endAt = Utc(b.FirstStartUtc.AddHours(1)), note = new string('n', 501),
        })));
        body = await AssertErrorAsync(longNote, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("note", out _), body.ToString());
    }

    [Fact]
    public async Task Block_can_be_created_then_deleted_and_a_second_delete_is_404_not_found()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var created = await owner.Http.PostAsync($"{Base}/blocks", Json(BlockBody(b.CourtId, b.FirstStartUtc, b.FirstStartUtc.AddHours(1), "cleaning")));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.NoContent, (await owner.Http.DeleteAsync($"{Base}/blocks/{id}")).StatusCode);
        await AssertErrorAsync(await owner.Http.DeleteAsync($"{Base}/blocks/{id}"), HttpStatusCode.NotFound, "not_found");
    }

    // ------------------------------------------------------------------ price rules

    [Theory]
    [InlineData("holiday", "03:00", "04:00", 100, "bad_day_type")]
    [InlineData("weekday", "abc", "04:00", 100, "bad_time")]
    [InlineData("weekday", "03:00", "nope", 100, "bad_time")]
    [InlineData("weekday", "04:00", "03:00", 100, "bad_range")]
    [InlineData("weekday", "03:00", "03:00", 100, "bad_range")]
    [InlineData("weekday", "03:00", "04:00", -1, "bad_price")]
    [InlineData("weekday", "03:00", "04:00", 100001, "bad_price")]
    public async Task Price_rule_domain_errors_keep_their_codes(string dayType, string start, string end, decimal price, string code)
    {
        var owner = await OwnerAsync();

        var res = await owner.Http.PostAsync($"{Base}/prices", Json(PriceBody(dayType, start, end, price)));

        await AssertErrorAsync(res, HttpStatusCode.BadRequest, code);
    }

    [Fact]
    public async Task Price_rule_for_an_unknown_court_is_404_court_not_found()
    {
        var owner = await OwnerAsync();

        var res = await owner.Http.PostAsync($"{Base}/prices", Json(PriceBody(courtId: Guid.NewGuid())));

        await AssertErrorAsync(res, HttpStatusCode.NotFound, "court_not_found");
    }

    [Fact]
    public async Task Price_rule_missing_required_text_is_validation_failed()
    {
        var owner = await OwnerAsync();

        var res = await owner.Http.PostAsync($"{Base}/prices", Json("""{ "pricePerHour": 100, "priority": 1 }"""));

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("dayType", out _) && errors.TryGetProperty("start", out _) && errors.TryGetProperty("end", out _), body.ToString());
    }

    [Fact]
    public async Task Ambiguous_price_rule_keeps_409_price_rule_ambiguous()
    {
        var owner = await OwnerAsync();
        // 03:00-04:00 is outside operating hours, so this rule never influences other tests' prices.
        var first = await owner.Http.PostAsync($"{Base}/prices", Json(PriceBody(priority: 7)));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var id = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        try
        {
            var clash = await owner.Http.PostAsync($"{Base}/prices", Json(PriceBody(priority: 7)));
            await AssertErrorAsync(clash, HttpStatusCode.Conflict, "price_rule_ambiguous");
        }
        finally
        {
            Assert.Equal(HttpStatusCode.NoContent, (await owner.Http.DeleteAsync($"{Base}/prices/{id}")).StatusCode);
        }
    }

    [Fact]
    public async Task Updating_or_deleting_an_unknown_price_rule_is_404_not_found()
    {
        var owner = await OwnerAsync();

        await AssertErrorAsync(await owner.Http.PutAsync($"{Base}/prices/{Guid.NewGuid()}", Json(PriceBody())), HttpStatusCode.NotFound, "not_found");
        await AssertErrorAsync(await owner.Http.DeleteAsync($"{Base}/prices/{Guid.NewGuid()}"), HttpStatusCode.NotFound, "not_found");
    }

    // ------------------------------------------------------------------ walk-ins

    [Fact]
    public async Task Walk_in_happy_path_then_the_same_slot_is_409_slot_taken()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var ok = await owner.Http.PostAsync($"{Base}/walk-ins", Json(WalkInBody(b, customerName: "Walk In")));
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);

        await AssertErrorAsync(await owner.Http.PostAsync($"{Base}/walk-ins", Json(WalkInBody(b))), HttpStatusCode.Conflict, "slot_taken");
    }

    [Fact]
    public async Task Walk_in_domain_errors_keep_their_codes()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        await AssertErrorAsync(
            await owner.Http.PostAsync($"{Base}/walk-ins", Json($$"""{ "date": "{{b.Date:yyyy-MM-dd}}", "slots": [] }""")),
            HttpStatusCode.BadRequest, "no_slots");
        await AssertErrorAsync(
            await owner.Http.PostAsync($"{Base}/walk-ins", Json(WalkInBody(b, start: "xx:yy"))),
            HttpStatusCode.BadRequest, "bad_time");
    }

    [Fact]
    public async Task Walk_in_overlong_customer_name_is_validation_failed()
    {
        var owner = await OwnerAsync();
        var b = SlotPicker.Next(api);

        var res = await owner.Http.PostAsync($"{Base}/walk-ins", Json(WalkInBody(b, customerName: new string('x', 101))));

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("customerName", out _), body.ToString());
    }

    // ------------------------------------------------------------------ authorization must win over validation

    [Fact]
    public async Task Non_member_staff_sending_an_invalid_body_gets_403_not_400()
    {
        var outsider = await StaffAsync(member: false);

        var res = await outsider.Http.PostAsync($"{Base}/prices", Json("{}"));

        await AssertErrorAsync(res, HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task Non_member_staff_on_walk_ins_with_an_invalid_body_gets_403_not_400()
    {
        var outsider = await StaffAsync(member: false);

        var res = await outsider.Http.PostAsync($"{Base}/walk-ins", Json($$"""{ "date": "2030-01-01", "slots": [ { "courtId": "{{Guid.Empty}}", "start": "" } ] }"""));

        await AssertErrorAsync(res, HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task Staff_who_is_not_owner_sending_an_invalid_price_rule_gets_403_not_400()
    {
        var staff = await StaffAsync(member: true);

        // a valid body is forbidden (owner-only)…
        await AssertErrorAsync(await staff.Http.PostAsync($"{Base}/prices", Json(PriceBody())), HttpStatusCode.Forbidden, "forbidden");
        // …and an invalid one must not leak validation details before the owner check
        await AssertErrorAsync(await staff.Http.PostAsync($"{Base}/prices", Json("{}")), HttpStatusCode.Forbidden, "forbidden");
    }

    // ------------------------------------------------------------------ query / binding failures

    [Theory]
    [InlineData("/bookings?date=garbage")]
    [InlineData("/bookings")]
    [InlineData("/blocks?date=2030-13-45")]
    public async Task Unparseable_or_missing_query_values_are_400_bad_request(string path)
    {
        var owner = await OwnerAsync();

        await AssertErrorAsync(await owner.Http.GetAsync(Base + path), HttpStatusCode.BadRequest, "bad_request");
    }

    [Theory]
    [InlineData("""{ "venueId": "not-a-guid", "date": "2030-01-01", "slots": [] }""")]
    [InlineData("""{ "venueId": "20000000-0000-0000-0000-000000000001", "date": "yesterday", "slots": [] }""")]
    [InlineData("""{ "venueId": "20000000-0000-0000-0000-000000000001", "date": "2030-01-01", "slots": "x" }""")]
    [InlineData("""{ "venueId": "20000000-0000-0000-0000-000000000001", "date": "2030-01-01", "slots": [], "addons": [ { "addonId": "20000000-0000-0000-0000-000000000001", "qty": "many" } ] }""")]
    public async Task Wrongly_typed_booking_json_is_400_bad_request(string raw)
    {
        var user = await api.LoginAsync();

        await AssertErrorAsync(await user.Http.PostAsync("/api/v1/bookings", Json(raw)), HttpStatusCode.BadRequest, "bad_request");
    }

    [Fact]
    public async Task Negative_or_huge_addon_qty_is_rejected_but_zero_and_fifty_pass_validation()
    {
        var user = await api.LoginAsync();
        var venue = Seed.VenueId;
        string Body(int qty) =>
            $$"""{ "venueId": "{{venue}}", "date": "2030-01-01", "slots": [], "addons": [ { "addonId": "{{Guid.NewGuid()}}", "qty": {{qty}} } ] }""";

        foreach (var bad in new[] { -1, 51 })
            await AssertErrorAsync(await user.Http.PostAsync("/api/v1/bookings", Json(Body(bad))), HttpStatusCode.BadRequest, "validation_failed");
        foreach (var ok in new[] { 0, 50 })
            await AssertErrorAsync(await user.Http.PostAsync("/api/v1/bookings", Json(Body(ok))), HttpStatusCode.BadRequest, "no_slots"); // past validation
    }

    [Fact]
    public async Task Too_many_slots_or_addons_are_capped_by_validation()
    {
        var user = await api.LoginAsync();
        var slots = string.Join(",", Enumerable.Range(0, 51).Select(_ => $$"""{ "courtId": "{{Seed.CourtIds[0]}}", "start": "06:00" }"""));

        var res = await user.Http.PostAsync("/api/v1/bookings", Json($$"""{ "venueId": "{{Seed.VenueId}}", "date": "2030-01-01", "slots": [ {{slots}} ] }"""));

        var body = await AssertErrorAsync(res, HttpStatusCode.BadRequest, "validation_failed");
        Assert.True(body.GetProperty("errors").TryGetProperty("slots", out _), body.ToString());
    }

    // ------------------------------------------------------------------ unexpected failures

    [Fact]
    public async Task Unexpected_exception_is_500_server_error_json_without_internal_details()
    {
        using var broken = api.WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:Default", "Host=127.0.0.1;Port=1;Database=nope;Username=x;Password=x;Timeout=2;Command Timeout=2"));

        var res = await broken.CreateClient().GetAsync("/api/v1/venues/" + Guid.NewGuid());

        var body = await AssertErrorAsync(res, HttpStatusCode.InternalServerError, "server_error");
        Assert.DoesNotContain("Npgsql", body.ToString());
        Assert.DoesNotContain("127.0.0.1", body.ToString());
        while (api.ServerErrors.TryDequeue(out _)) { } // the 500 above was deliberate; do not leak it into other tests' diagnostics
    }

    // ------------------------------------------------------------------ ValidationFilter must only ever inspect DTOs

    [Fact]
    public void ValidationFilter_never_treats_services_or_framework_arguments_as_validatable()
    {
        var hasRules = typeof(ValidationFilter).GetMethod("HasRules", BindingFlags.NonPublic | BindingFlags.Static, [typeof(Type)])!;
        bool Has(Type t) => (bool)hasRules.Invoke(null, [t])!;

        // endpoint arguments that are not request bodies: enumerating their properties (e.g. DbSet) could hit the database
        foreach (var t in new[]
                 {
                     typeof(AppDbContext), typeof(BookingService), typeof(AvailabilityService), typeof(AvailabilityNotifier),
                     typeof(ClaimsPrincipal), typeof(HttpContext), typeof(CancellationToken), typeof(TimeProvider),
                     typeof(IWebHostEnvironment), typeof(ILoggerFactory), typeof(Guid), typeof(DateOnly), typeof(string), typeof(int),
                 })
            Assert.False(Has(t), $"{t.Name} must not be validated");

        foreach (var t in new[] { typeof(OtpRequest), typeof(OtpVerify), typeof(CreateBookingRequest), typeof(SlotRequest), typeof(AddonRequest),
                     typeof(WalkInRequest), typeof(CreateBlockRequest), typeof(PriceRuleRequest) })
            Assert.True(Has(t), $"{t.Name} should carry validation rules");
    }
}
