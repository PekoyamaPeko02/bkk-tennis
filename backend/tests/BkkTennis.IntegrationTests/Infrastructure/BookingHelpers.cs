using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Services;

namespace BkkTennis.IntegrationTests.Infrastructure;

/// <summary>Seeded venue used by booking tests: The Racquet Club Sukhumvit 49 (5 courts, 06–22h, 450/h before 17:00).</summary>
public static class Seed
{
    public static readonly Guid VenueId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public const string VenueSlug = "racquet-club-sukhumvit-49";
    public const string Timezone = "Asia/Bangkok";
    public const decimal StandardHourly = 450m;
    public static readonly Guid[] CourtIds = Enumerable.Range(1, 5)
        .Select(i => Guid.Parse($"30000000-0000-0000-0000-00000000001{i}")).ToArray();
}

/// <summary>A court + two consecutive hourly slots (all before 17:00, so 450/h) on a given venue-local date.</summary>
public sealed record SlotBlock(Guid CourtId, DateOnly Date, string First, string Second)
{
    public DateTime FirstStartUtc => VenueClock.ToUtc(Date, TimeOnly.Parse(First), Seed.Timezone);
}

public static class SlotPicker
{
    private static int _next = -1;

    /// <summary>
    /// Hands out a block nobody else has used: 5 courts × 5 two-hour blocks (06–16h) per day, starting 3 days ahead
    /// (beyond the 4 h free-cancel window) and moving one day further every 25 blocks. The shared DB is never reset
    /// between tests, so every test takes its own block. Max advance is 14 days ⇒ at most ~275 blocks per run.
    /// Relies on the fake clock only moving forward and on tests in the "api" collection running serially.
    /// </summary>
    public static SlotBlock Next(ApiFactory api)
    {
        var i = Interlocked.Increment(ref _next);
        var today = VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
        var hour = 6 + 2 * ((i / 5) % 5);
        return new SlotBlock(Seed.CourtIds[i % 5], today.AddDays(3 + i / 25), $"{hour:00}:00", $"{hour + 1:00}:00");
    }
}

public static class BookingClient
{
    public static object Request(SlotBlock b, int slots = 1, IEnumerable<(Guid AddonId, int Qty)>? addons = null) => new
    {
        venueId = Seed.VenueId,
        date = b.Date.ToString("yyyy-MM-dd"),
        slots = new[] { b.First, b.Second }.Take(slots).Select(s => new { courtId = b.CourtId, start = s }).ToArray(),
        addons = addons?.Select(a => new { addonId = a.AddonId, qty = a.Qty }).ToArray(),
    };

    public static Task<HttpResponseMessage> HoldAsync(this HttpClient http, SlotBlock b, int slots = 1,
        string? idempotencyKey = null, IEnumerable<(Guid, int)>? addons = null)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bookings") { Content = JsonContent.Create(Request(b, slots, addons)) };
        if (idempotencyKey is not null) msg.Headers.Add("Idempotency-Key", idempotencyKey);
        return http.SendAsync(msg);
    }

    /// <summary>Holds and asserts 201; returns the booking JSON.</summary>
    public static async Task<JsonElement> HoldOkAsync(this HttpClient http, SlotBlock b, int slots = 1)
    {
        var res = await http.HoldAsync(b, slots);
        Assert.True(res.StatusCode == System.Net.HttpStatusCode.Created, await res.Content.ReadAsStringAsync());
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<JsonElement> GetBookingAsync(this HttpClient http, Guid id) =>
        await http.GetFromJsonAsync<JsonElement>($"/api/v1/me/bookings/{id}");

    /// <summary>Dev endpoint that plays the payment webhook. Returns the raw response.</summary>
    public static Task<HttpResponseMessage> SimulatePaymentAsync(this HttpClient http, Guid bookingId) =>
        http.PostAsync($"/api/v1/dev/bookings/{bookingId}/simulate-payment", null);

    public static Guid Id(this JsonElement booking) => booking.GetProperty("id").GetGuid();
    public static string Status(this JsonElement booking) => booking.GetProperty("status").GetString()!;
    public static string ErrorCode(this JsonElement error) => error.GetProperty("code").GetString()!;
}
