using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// Seed geography (from The Racquet Club, 13.7358,100.5783): Benchasiri ≈1.2 km, Asoke ≈1.9 km, Ari ≈6.1 km.
/// Read-only tests may share venues; tests that book use a venue nobody else books (Ari / Asoke) so dates never clash
/// even though the fake clock moves between tests.
/// </summary>
[Collection(ApiCollection.Name)]
public class VenueSearchTests(ApiFactory api)
{
    private const string Racquet = "racquet-club-sukhumvit-49", Benchasiri = "benchasiri-park", Asoke = "asoke-sports-club", Ari = "ari-tennis-hub";
    private static readonly string[] AllSlugs = [Racquet, Benchasiri, Asoke, Ari];
    private const double RacquetLat = 13.7358, RacquetLng = 100.5783;

    private static readonly Guid AriVenueId = Guid.Parse("20000000-0000-0000-0000-000000000004");
    private static readonly Guid AriCourt = Guid.Parse("30000000-0000-0000-0000-000000000041");
    private static readonly Guid AsokeVenueId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid AsokeClay1 = Guid.Parse("30000000-0000-0000-0000-000000000031");

    private async Task<JsonElement> SearchAsync(string query = "")
    {
        var res = await api.Anonymous().GetAsync("/api/v1/venues" + (query.Length > 0 ? "?" + query : ""));
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<List<JsonElement>> ItemsAsync(string query = "") =>
        (await SearchAsync(query)).GetProperty("items").EnumerateArray().ToList();

    private static string[] Slugs(IEnumerable<JsonElement> items) => items.Select(i => i.GetProperty("slug").GetString()!).ToArray();

    private static string Near(double km) => $"lat={RacquetLat}&lng={RacquetLng}&radiusKm={km}";

    private string Today() => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime).ToString("yyyy-MM-dd");
    private DateOnly TodayDate() => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);

    // ------------------------------------------------------------------ radius (PostGIS)

    [Fact]
    public async Task Without_a_location_all_active_venues_are_returned_with_no_distance()
    {
        var items = await ItemsAsync();

        Assert.Equal(AllSlugs.Order(), Slugs(items).Order());
        Assert.All(items, i => Assert.Equal(JsonValueKind.Null, i.GetProperty("distanceKm").ValueKind));
    }

    [Theory]
    [InlineData(1.0, new[] { Racquet })]
    [InlineData(1.5, new[] { Racquet, Benchasiri })]
    [InlineData(3.0, new[] { Racquet, Benchasiri, Asoke })]
    [InlineData(8.0, new[] { Racquet, Benchasiri, Asoke, Ari })]
    public async Task Radius_keeps_only_venues_within_range(double radiusKm, string[] expected)
    {
        var items = await ItemsAsync(Near(radiusKm));

        Assert.Equal(expected.Order(), Slugs(items).Order());
    }

    [Fact]
    public async Task Distance_is_geodesic_km_rounded_to_one_decimal_and_default_sort_is_nearest_first()
    {
        var items = await ItemsAsync(Near(8));

        Assert.Equal(new[] { Racquet, Benchasiri, Asoke, Ari }, Slugs(items));
        var km = items.Select(i => i.GetProperty("distanceKm").GetDouble()).ToArray();
        Assert.Equal(0.0, km[0]);
        Assert.InRange(km[1], 1.1, 1.4);
        Assert.InRange(km[2], 1.7, 2.1);
        Assert.InRange(km[3], 5.8, 6.4);
        Assert.All(km, k => Assert.Equal(Math.Round(k, 1), k));
    }

    [Fact]
    public async Task Radius_below_the_minimum_is_clamped_to_half_a_km()
    {
        // 0.1 km is clamped up to 0.5 km: the venue at distance 0 is kept, Benchasiri (≈1.2 km) is not.
        Assert.Equal(new[] { Racquet }, Slugs(await ItemsAsync(Near(0.1))));
    }

    [Fact]
    public async Task Tiny_radius_is_clamped_up_to_half_a_km_for_a_venue_between_the_two()
    {
        // Search point ≈333 m north of The Racquet Club: outside 0.1 km, inside the 0.5 km minimum.
        var items = await ItemsAsync($"lat={RacquetLat + 0.003}&lng={RacquetLng}&radiusKm=0.1");

        Assert.Equal(new[] { Racquet }, Slugs(items));
        Assert.InRange(items[0].GetProperty("distanceKm").GetDouble(), 0.2, 0.5);
    }

    [Fact]
    public async Task Huge_radius_is_clamped_to_sixty_km()
    {
        // Chiang Mai is ≈580 km from Bangkok: even radiusKm=1000 must not reach it.
        var res = await SearchAsync("lat=18.7883&lng=98.9853&radiusKm=1000");

        Assert.Equal(0, res.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Location_without_radius_defaults_to_five_km()
    {
        var items = await ItemsAsync($"lat={RacquetLat}&lng={RacquetLng}");

        Assert.Equal(new[] { Racquet, Benchasiri, Asoke }, Slugs(items)); // Ari is ≈6.1 km away
    }

    [Fact]
    public async Task Far_away_location_finds_nothing()
    {
        var res = await SearchAsync("lat=18.7883&lng=98.9853&radiusKm=10"); // Chiang Mai

        Assert.Equal(0, res.GetProperty("count").GetInt32());
        Assert.Empty(res.GetProperty("items").EnumerateArray());
    }

    // ------------------------------------------------------------------ surface / indoor / text filters

    [Theory]
    [InlineData("hard", new[] { Racquet, Benchasiri, Ari })]
    [InlineData("clay", new[] { Racquet, Asoke })]
    [InlineData("grass", new string[0])]
    public async Task Surface_filter_matches_venues_with_at_least_one_court_of_that_surface(string surface, string[] expected)
    {
        Assert.Equal(expected.Order(), Slugs(await ItemsAsync($"surface={surface}")).Order());
    }

    [Fact]
    public async Task Indoor_filter_keeps_only_venues_with_an_indoor_court()
    {
        var items = await ItemsAsync("indoor=true");

        Assert.Equal(new[] { Racquet }, Slugs(items));
        Assert.True(items[0].GetProperty("hasIndoor").GetBoolean());
    }

    [Fact]
    public async Task Indoor_false_does_not_filter()
    {
        Assert.Equal(4, (await ItemsAsync("indoor=false")).Count);
    }

    [Fact]
    public async Task Filters_combine_with_radius()
    {
        // clay within 1 km of The Racquet Club → only The Racquet Club (Asoke is ≈1.9 km away)
        Assert.Equal(new[] { Racquet }, Slugs(await ItemsAsync(Near(1) + "&surface=clay")));
    }

    [Theory]
    [InlineData("asoke", new[] { Asoke })]
    [InlineData("ARI", new[] { Ari })]           // case-insensitive, name_en
    [InlineData("Phaya", new[] { Ari })]         // district
    [InlineData("เบญจสิริ", new[] { Benchasiri })] // name_th
    [InlineData("nothing-like-this", new string[0])]
    public async Task Text_query_matches_name_or_district_case_insensitively(string q, string[] expected)
    {
        Assert.Equal(expected, Slugs(await ItemsAsync($"q={Uri.EscapeDataString(q)}")));
    }

    // ------------------------------------------------------------------ sorting

    [Fact]
    public async Task Sort_by_price_is_cheapest_starting_price_first()
    {
        var items = await ItemsAsync("sort=price");

        Assert.Equal(new[] { Benchasiri, Ari, Racquet, Asoke }, Slugs(items)); // 150, 300, 450, 500
        Assert.Equal(new decimal[] { 150, 300, 450, 500 }, items.Select(i => i.GetProperty("priceFrom").GetDecimal()).ToArray());
    }

    [Fact]
    public async Task Sort_by_rating_is_highest_first()
    {
        Assert.Equal(new[] { Racquet, Asoke, Benchasiri, Ari }, Slugs(await ItemsAsync("sort=rating"))); // 4.9, 4.8, 4.6, 4.5
    }

    [Fact]
    public async Task Sort_by_distance_orders_from_the_given_point()
    {
        // From Ari: Ari 0 km, Asoke ≈4.6 km, then Benchasiri and The Racquet Club (both ≈6.1 km, too close to order reliably).
        var items = await ItemsAsync("lat=13.7797&lng=100.5446&radiusKm=10&sort=distance");

        Assert.Equal(new[] { Ari, Asoke }, Slugs(items)[..2]);
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public async Task Sort_by_earliest_puts_the_venue_whose_first_free_slot_is_latest_last()
    {
        // Ari has a single court; take its 06:00 slot on a date nobody else books at Ari.
        var date = TodayDate().AddDays(9);
        var user = await api.LoginAsync();
        await HoldAsync(user, AriVenueId, AriCourt, date, "06:00");

        var res = await SearchAsync($"sort=earliest&date={date:yyyy-MM-dd}");

        var items = res.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(Ari, Slugs(items)[^1]);
        var ari = items.Single(i => i.GetProperty("slug").GetString() == Ari);
        Assert.Equal("07:00", ari.GetProperty("nextFreeTimes")[0].GetString());
        Assert.Equal(15, ari.GetProperty("freeSlotCount").GetInt32()); // 16 hourly slots minus the held one
        Assert.All(items.Where(i => i.GetProperty("slug").GetString() != Ari),
            i => Assert.Equal("06:00", i.GetProperty("nextFreeTimes")[0].GetString()));
    }

    // ------------------------------------------------------------------ time window / free slots

    [Fact]
    public async Task Free_slots_default_to_the_whole_day_and_report_next_three_start_times()
    {
        var date = TodayDate().AddDays(11).ToString("yyyy-MM-dd");

        var ben = (await ItemsAsync($"date={date}")).Single(i => i.GetProperty("slug").GetString() == Benchasiri);

        Assert.Equal(2 * 16, ben.GetProperty("freeSlotCount").GetInt32()); // 2 courts × 06:00–22:00
        Assert.Equal(new[] { "06:00", "07:00", "08:00" },ben.GetProperty("nextFreeTimes").EnumerateArray().Select(x => x.GetString()).ToArray());
    }

    [Fact]
    public async Task Time_window_counts_slots_starting_inside_from_inclusive_to_exclusive()
    {
        var date = TodayDate().AddDays(11).ToString("yyyy-MM-dd");

        var ben = (await ItemsAsync($"date={date}&from=17:00&to=19:00")).Single(i => i.GetProperty("slug").GetString() == Benchasiri);

        Assert.Equal(2 * 2, ben.GetProperty("freeSlotCount").GetInt32()); // 17:00 and 18:00 on both courts; 19:00 excluded
        Assert.Equal(new[] { "17:00", "18:00" },ben.GetProperty("nextFreeTimes").EnumerateArray().Select(x => x.GetString()).ToArray());
    }

    [Fact]
    public async Task Past_days_have_no_free_slots()
    {
        var yesterday = TodayDate().AddDays(-1).ToString("yyyy-MM-dd");

        var items = await ItemsAsync($"date={yesterday}");

        Assert.All(items, i => Assert.Equal(0, i.GetProperty("freeSlotCount").GetInt32()));
        Assert.All(items, i => Assert.Empty(i.GetProperty("nextFreeTimes").EnumerateArray()));
    }

    [Fact]
    public async Task Response_echoes_the_requested_date_and_a_matching_count()
    {
        var date = TodayDate().AddDays(4).ToString("yyyy-MM-dd");

        var res = await SearchAsync($"date={date}&surface=clay");

        Assert.Equal(date, res.GetProperty("date").GetString());
        Assert.Equal(res.GetProperty("items").GetArrayLength(), res.GetProperty("count").GetInt32());
    }

    // ------------------------------------------------------------------ availability endpoint

    [Fact]
    public async Task Availability_reports_price_label_and_status_per_slot()
    {
        var date = TodayDate().AddDays(10);
        var user = await api.LoginAsync();

        var before = await AvailabilityAsync(AsokeVenueId, date);
        Assert.Equal(2, before.GetProperty("courts").GetArrayLength());
        var slot = Slot(before, AsokeClay1, "18:00");
        Assert.Equal("available", slot.GetProperty("status").GetString());
        Assert.Equal(500m, slot.GetProperty("price").GetDecimal());
        Assert.Equal("Red Clay", slot.GetProperty("label").GetString());
        Assert.Equal(60, before.GetProperty("slotMinutes").GetInt32());

        var held = await HoldAsync(user, AsokeVenueId, AsokeClay1, date, "18:00");
        Assert.Equal("held", Slot(await AvailabilityAsync(AsokeVenueId, date), AsokeClay1, "18:00").GetProperty("status").GetString());
        Assert.Equal("available", Slot(await AvailabilityAsync(AsokeVenueId, date), AsokeClay1, "19:00").GetProperty("status").GetString());
        Assert.Equal("available", Slot(await AvailabilityAsync(AsokeVenueId, date), Guid.Parse("30000000-0000-0000-0000-000000000032"), "18:00")
            .GetProperty("status").GetString());

        await user.Http.SimulatePaymentAsync(held.Id());
        Assert.Equal("booked", Slot(await AvailabilityAsync(AsokeVenueId, date), AsokeClay1, "18:00").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Availability_marks_started_slots_as_past()
    {
        var res = await AvailabilityAsync(AsokeVenueId, TodayDate().AddDays(-1));

        var all = res.GetProperty("courts").EnumerateArray().SelectMany(c => c.GetProperty("slots").EnumerateArray());
        Assert.All(all, s => Assert.Equal("past", s.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task Availability_treats_a_slot_starting_exactly_now_as_past()
    {
        // The fake clock only moves forward: jump to the next 10:00 venue-local (at most a day ahead, same as other tests do).
        var date = TodayDate();
        var ten = VenueClock.ToUtc(date, TimeOnly.Parse("10:00"), Seed.Timezone);
        if (ten <= api.Clock.GetUtcNow().UtcDateTime)
        {
            date = date.AddDays(1);
            ten = VenueClock.ToUtc(date, TimeOnly.Parse("10:00"), Seed.Timezone);
        }
        api.Clock.SetUtcNow(new DateTimeOffset(DateTime.SpecifyKind(ten, DateTimeKind.Utc)));

        var res = await AvailabilityAsync(AsokeVenueId, date);

        Assert.Equal("past", Slot(res, AsokeClay1, "10:00").GetProperty("status").GetString());
        Assert.Equal("available", Slot(res, AsokeClay1, "11:00").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Availability_uses_time_dependent_price_rules()
    {
        var res = await AvailabilityAsync(AriVenueId, TodayDate().AddDays(12));

        var day = Slot(res, AriCourt, "10:00");
        var evening = Slot(res, AriCourt, "17:00");
        Assert.Equal((300m, "Day"), (day.GetProperty("price").GetDecimal(), day.GetProperty("label").GetString()));
        Assert.Equal((400m, "Evening"), (evening.GetProperty("price").GetDecimal(), evening.GetProperty("label").GetString()));
    }

    [Fact]
    public async Task Availability_for_an_unknown_venue_is_404()
    {
        var res = await api.Anonymous().GetAsync($"/api/v1/venues/{Guid.NewGuid()}/availability?date={Today()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ------------------------------------------------------------------ detail / 404 / drafts

    [Fact]
    public async Task Detail_is_found_by_slug_and_by_id()
    {
        var bySlug = await api.Anonymous().GetFromJsonAsync<JsonElement>($"/api/v1/venues/{Racquet}");
        var byId = await api.Anonymous().GetFromJsonAsync<JsonElement>($"/api/v1/venues/{Seed.VenueId}");

        Assert.Equal(Seed.VenueId, bySlug.GetProperty("summary").GetProperty("id").GetGuid());
        Assert.Equal(bySlug.GetProperty("summary").GetProperty("slug").GetString(), byId.GetProperty("summary").GetProperty("slug").GetString());
        Assert.Equal(5, bySlug.GetProperty("courts").GetArrayLength());
        Assert.Equal(4, bySlug.GetProperty("cancelFreeHours").GetInt32());
        Assert.Equal(3, bySlug.GetProperty("addons").GetArrayLength());
        Assert.Equal(450m, bySlug.GetProperty("summary").GetProperty("priceFrom").GetDecimal());
        Assert.Equal(650m, bySlug.GetProperty("summary").GetProperty("priceTo").GetDecimal());
    }

    [Fact]
    public async Task Detail_with_a_location_includes_the_distance()
    {
        var res = await api.Anonymous().GetFromJsonAsync<JsonElement>($"/api/v1/venues/{Benchasiri}?lat={RacquetLat}&lng={RacquetLng}");

        Assert.InRange(res.GetProperty("summary").GetProperty("distanceKm").GetDouble(), 1.1, 1.4);
    }

    [Theory]
    [InlineData("no-such-venue")]
    [InlineData("00000000-0000-0000-0000-00000000dead")]
    public async Task Unknown_venue_is_404(string idOrSlug)
    {
        var res = await api.Anonymous().GetAsync($"/api/v1/venues/{idOrSlug}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Draft_venues_are_hidden_from_search_detail_and_radius()
    {
        var slug = "draft-" + Guid.NewGuid().ToString("N")[..8];
        await api.WithDb(db => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO venues (partner_id, slug, name_th, name_en, district, address, lat, lng, status)
            VALUES ('10000000-0000-0000-0000-000000000001', {0}, 'ร่าง', 'Draft Court', 'Watthana', 'x', 13.7358, 100.5783, 'draft')
            """, slug));

        Assert.DoesNotContain(slug, Slugs(await ItemsAsync()));
        Assert.DoesNotContain(slug, Slugs(await ItemsAsync(Near(1)))); // same coordinates as The Racquet Club
        Assert.DoesNotContain(slug, Slugs(await ItemsAsync("q=Draft")));
        Assert.Equal(HttpStatusCode.NotFound, (await api.Anonymous().GetAsync($"/api/v1/venues/{slug}")).StatusCode);
    }

    // ------------------------------------------------------------------ helpers

    private async Task<JsonElement> AvailabilityAsync(Guid venueId, DateOnly date) =>
        await api.Anonymous().GetFromJsonAsync<JsonElement>($"/api/v1/venues/{venueId}/availability?date={date:yyyy-MM-dd}");

    private static JsonElement Slot(JsonElement availability, Guid courtId, string start) =>
        availability.GetProperty("courts").EnumerateArray().Single(c => c.GetProperty("courtId").GetGuid() == courtId)
            .GetProperty("slots").EnumerateArray().Single(s => s.GetProperty("start").GetString() == start);

    private static async Task<JsonElement> HoldAsync(TestUser user, Guid venueId, Guid courtId, DateOnly date, string start)
    {
        var res = await user.Http.PostAsJsonAsync("/api/v1/bookings", new
        {
            venueId,
            date = date.ToString("yyyy-MM-dd"),
            slots = new[] { new { courtId, start } },
        });
        Assert.True(res.StatusCode == HttpStatusCode.Created, await res.Content.ReadAsStringAsync());
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }
}
