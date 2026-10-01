using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// C5: /clubos/venues/{id}/settings, /courts, /hours, /addons, /prices/preview. Reads = any member, writes = owner only.
/// Every test owns a private venue (ScheduleVenue). Tests that need the PUBLIC endpoints (availability, venue detail,
/// booking) flip that venue to 'active' for the duration of the test only, so venue search tests never see it.
/// </summary>
[Collection(ApiCollection.Name)]
public class SettingsTests(ApiFactory api)
{
    private DateOnly Today => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
    private DateOnly Future => Today.AddDays(5);

    private DateOnly Next(DayOfWeek day)
    {
        var d = Today.AddDays(2);
        while (d.DayOfWeek != day) d = d.AddDays(1);
        return d;
    }

    private static string Base(ScheduleVenue v) => $"/api/v1/clubos/venues/{v.Id}";
    private static StringContent Json(string raw) => new(raw, Encoding.UTF8, "application/json");

    private async Task<TestUser> OwnerAsync(ScheduleVenue v) => await api.LoginPartnerAsync(v.Id, "owner", "partner_owner");

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage res, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"expected {(int)expected} but got {(int)res.StatusCode}: {raw}");
        return raw.Length == 0 ? default : JsonDocument.Parse(raw).RootElement;
    }

    private static async Task AssertErrorAsync(HttpResponseMessage res, HttpStatusCode status, string code)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} but got {(int)res.StatusCode}: {raw}");
        Assert.Equal(code, JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
    }

    private static Task<JsonElement> SettingsAsync(HttpClient http, ScheduleVenue v) => http.GetAsync(Base(v) + "/settings").ContinueWith(t => ReadAsync(t.Result)).Unwrap();

    private async Task WithActiveAsync(ScheduleVenue v, Func<Task> body)
    {
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE venues SET status = 'active' WHERE id = {v.Id}"));
        try { await body(); }
        finally { await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE venues SET status = 'hidden' WHERE id = {v.Id}")); }
    }

    private static object CourtBody(string name = "คอร์ท C", string surface = "grass", bool active = true, int sort = 3) =>
        new { name, surface, indoor = false, airCon = false, floodlit = true, sortOrder = sort, isActive = active };

    private static object AddonBody(string name = "ไม้เช่า", string unit = "per_booking", decimal price = 100, bool active = true) =>
        new { name, description = "desc", icon = "sports_tennis", price, unit, isActive = active };

    private static object Day(int d, string open = "06:00", string close = "22:00", int slot = 60) =>
        new { dayOfWeek = d, open, close, slotMinutes = slot };

    private static object Hours(params object[] days) => new { days };

    private async Task<Guid> CreateCourtAsync(ScheduleVenue v, TestUser owner, object? body = null)
    {
        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/courts", body ?? CourtBody());
        return (await ReadAsync(res, HttpStatusCode.Created)).GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> AvailabilityAsync(HttpClient http, ScheduleVenue v, DateOnly date) =>
        await ReadAsync(await http.GetAsync($"/api/v1/venues/{v.Id}/availability?date={date:yyyy-MM-dd}"));

    // ------------------------------------------------------------------ authorization

    private static readonly Guid Benchasiri = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid AnyId = Guid.NewGuid();

    public static IEnumerable<object[]> Writes() =>
    [
        ["PUT", "/settings/cancel-policy"],
        ["POST", "/courts"],
        ["PUT", "/courts/" + "{id}"],
        ["PUT", "/hours"],
        ["POST", "/addons"],
        ["PUT", "/addons/" + "{id}"],
    ];

    public static IEnumerable<object[]> Reads() => [["/settings"], ["/prices/preview?date=2030-01-01"]];

    private static Task<HttpResponseMessage> Call(HttpClient http, ScheduleVenue v, string method, string path, string body = "{}") =>
        http.SendAsync(new HttpRequestMessage(new HttpMethod(method), Base(v) + path.Replace("{id}", AnyId.ToString())) { Content = Json(body) });

    [Theory]
    [MemberData(nameof(Writes))]
    public async Task Front_desk_staff_get_403_on_every_write_even_with_an_invalid_body(string method, string path)
    {
        var v = await ScheduleVenue.CreateAsync(api);

        // "{}" and a court id that does not exist: authorization must win over validation (400) and lookup (404).
        await AssertErrorAsync(await Call(v.Staff.Http, v, method, path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Theory]
    [MemberData(nameof(Writes))]
    public async Task Anonymous_callers_get_401_on_every_write(string method, string path)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await AssertErrorAsync(await Call(api.Anonymous(), v, method, path), HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Theory]
    [MemberData(nameof(Writes))]
    public async Task Players_and_other_venues_staff_get_403_on_every_write(string method, string path)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var otherOwner = await api.LoginPartnerAsync(Benchasiri, "owner", "partner_owner");

        await AssertErrorAsync(await Call(player.Http, v, method, path), HttpStatusCode.Forbidden, "forbidden");
        await AssertErrorAsync(await Call(otherOwner.Http, v, method, path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Theory]
    [MemberData(nameof(Reads))]
    public async Task Reads_are_open_to_staff_and_closed_to_anonymous_players_and_other_venues(string path)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var otherStaff = await api.LoginPartnerAsync(Benchasiri);

        Assert.Equal(HttpStatusCode.OK, (await v.Staff.Http.GetAsync(Base(v) + path)).StatusCode);
        await AssertErrorAsync(await api.Anonymous().GetAsync(Base(v) + path), HttpStatusCode.Unauthorized, "unauthorized");
        await AssertErrorAsync(await player.Http.GetAsync(Base(v) + path), HttpStatusCode.Forbidden, "forbidden");
        await AssertErrorAsync(await otherStaff.Http.GetAsync(Base(v) + path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task Staff_writes_change_nothing()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        await v.Staff.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = 99 });
        await v.Staff.Http.PostAsJsonAsync(Base(v) + "/courts", CourtBody());
        await v.Staff.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody());
        await v.Staff.Http.PutAsJsonAsync(Base(v) + "/hours", Hours());

        var s = await SettingsAsync(v.Staff.Http, v);
        Assert.Equal(4, s.GetProperty("cancelFreeHours").GetInt32());
        Assert.Equal(2, s.GetProperty("courts").GetArrayLength());
        Assert.Equal(0, s.GetProperty("addons").GetArrayLength());
        Assert.Equal(7, s.GetProperty("hours").GetArrayLength());
    }

    // ------------------------------------------------------------------ GET /settings

    [Fact]
    public async Task Settings_lists_courts_in_sort_order_hours_by_day_and_the_default_cancel_window()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.SetCourtActiveAsync(v.CourtB, false); // inactive courts are still listed for the owner

        var s = await SettingsAsync(v.Staff.Http, v);

        Assert.Equal(4, s.GetProperty("cancelFreeHours").GetInt32());
        var courts = s.GetProperty("courts").EnumerateArray().ToList();
        Assert.Equal(new[] { v.CourtA, v.CourtB }, courts.Select(c => c.GetProperty("id").GetGuid()).ToArray());
        Assert.Equal("hard", courts[0].GetProperty("surface").GetString());
        Assert.True(courts[0].GetProperty("indoor").GetBoolean());
        Assert.True(courts[0].GetProperty("isActive").GetBoolean());
        Assert.False(courts[1].GetProperty("isActive").GetBoolean());
        var hours = s.GetProperty("hours").EnumerateArray().ToList();
        Assert.Equal(Enumerable.Range(0, 7), hours.Select(h => h.GetProperty("dayOfWeek").GetInt32()));
        Assert.All(hours, h =>
        {
            Assert.Equal("06:00", h.GetProperty("open").GetString());
            Assert.Equal("22:00", h.GetProperty("close").GetString());
            Assert.Equal(60, h.GetProperty("slotMinutes").GetInt32());
        });
        Assert.Empty(s.GetProperty("addons").EnumerateArray());
    }

    [Fact]
    public async Task Settings_only_shows_this_venues_data()
    {
        var a = await ScheduleVenue.CreateAsync(api);
        var b = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(a);
        await CreateCourtAsync(a, owner, CourtBody("เฉพาะ A"));

        var sb = await SettingsAsync(b.Staff.Http, b);

        Assert.Equal(2, sb.GetProperty("courts").GetArrayLength());
    }

    // ------------------------------------------------------------------ cancel policy

    [Theory]
    [InlineData(0)]
    [InlineData(24)]
    [InlineData(168)]
    public async Task Cancel_policy_accepts_0_to_168_and_persists(int hours)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = hours });

        Assert.Equal(hours, (await ReadAsync(res)).GetProperty("cancelFreeHours").GetInt32());
        Assert.Equal(hours, (await SettingsAsync(v.Staff.Http, v)).GetProperty("cancelFreeHours").GetInt32());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(169)]
    [InlineData(100000)]
    public async Task Cancel_policy_outside_0_to_168_is_400_bad_cancel_hours_and_unchanged(int hours)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = hours }),
            HttpStatusCode.BadRequest, "bad_cancel_hours");
        Assert.Equal(4, (await SettingsAsync(v.Staff.Http, v)).GetProperty("cancelFreeHours").GetInt32());
    }

    [Fact]
    public async Task Cancel_policy_of_a_venue_that_does_not_exist_is_unreachable_403()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PutAsJsonAsync($"/api/v1/clubos/venues/{Guid.NewGuid()}/settings/cancel-policy", new { cancelFreeHours = 3 });

        await AssertErrorAsync(res, HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task A_changed_cancel_window_decides_the_refund_of_a_new_cancellation()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var date = Today.AddDays(2); // 24-72 h from now, whatever time of day the test runs

        await WithActiveAsync(v, async () =>
        {
            async Task<decimal> BookPayAndCancelAsync(Func<Task>? between)
            {
                var player = await api.LoginAsync();
                var slot = new { courtId = v.CourtA, start = "10:00" };
                var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings",
                    new { venueId = v.Id, date = date.ToString("yyyy-MM-dd"), slots = new[] { slot } }), HttpStatusCode.Created);
                var id = hold.Id();
                (await player.Http.SimulatePaymentAsync(id)).EnsureSuccessStatusCode();
                if (between is not null) await between();
                var cancel = await ReadAsync(await player.Http.PostAsync($"/api/v1/me/bookings/{id}/cancel", null));
                Assert.Equal("cancelled", cancel.GetProperty("booking").Status());
                return cancel.GetProperty("refund").GetDecimal();
            }

            // default 4 h: cancelled ≥ 24 h ahead → full refund
            Assert.Equal(400m, await BookPayAndCancelAsync(null));

            // window widened to 7 days AFTER the booking was paid: this cancellation is now inside the window → no refund
            Assert.Equal(0m, await BookPayAndCancelAsync(async () =>
                (await owner.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = 168 })).EnsureSuccessStatusCode()));

            // narrowed to 0 h → refund again
            Assert.Equal(400m, await BookPayAndCancelAsync(async () =>
                (await owner.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = 0 })).EnsureSuccessStatusCode()));
        });
    }

    [Fact]
    public async Task The_public_venue_detail_shows_the_new_cancel_window()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        (await owner.Http.PutAsJsonAsync(Base(v) + "/settings/cancel-policy", new { cancelFreeHours = 12 })).EnsureSuccessStatusCode();

        await WithActiveAsync(v, async () =>
        {
            var detail = await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"));
            Assert.Equal(12, detail.GetProperty("cancelFreeHours").GetInt32());
        });
    }

    // ------------------------------------------------------------------ courts

    [Fact]
    public async Task Creating_a_court_returns_201_lists_it_and_makes_it_bookable_at_the_venue_price()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var id = await CreateCourtAsync(v, owner, CourtBody("  คอร์ท หญ้า  ", "grass"));

        var court = (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").EnumerateArray().Single(c => c.GetProperty("id").GetGuid() == id);
        Assert.Equal("คอร์ท หญ้า", court.GetProperty("name").GetString()); // trimmed
        Assert.Equal("grass", court.GetProperty("surface").GetString());
        Assert.True(court.GetProperty("floodlit").GetBoolean());
        Assert.True(court.GetProperty("isActive").GetBoolean());

        await WithActiveAsync(v, async () =>
        {
            var courts = (await AvailabilityAsync(api.Anonymous(), v, Future)).GetProperty("courts").EnumerateArray().ToList();
            Assert.Equal(3, courts.Count);
            var fresh = courts.Single(c => c.GetProperty("courtId").GetGuid() == id);
            var ten = fresh.GetProperty("slots").EnumerateArray().Single(s => s.GetProperty("start").GetString() == "10:00");
            Assert.Equal(400m, ten.GetProperty("price").GetDecimal()); // no court rule → venue rule
            Assert.Equal("available", ten.GetProperty("status").GetString());

            var player = await api.LoginAsync();
            var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings",
                new { venueId = v.Id, date = Future.ToString("yyyy-MM-dd"), slots = new[] { new { courtId = id, start = "10:00" } } }),
                HttpStatusCode.Created);
            Assert.Equal(400m, hold.GetProperty("total").GetDecimal());

            var detail = await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"));
            Assert.Contains(id, detail.GetProperty("courts").EnumerateArray().Select(c => c.GetProperty("id").GetGuid()));
        });
    }

    [Theory]
    [InlineData("hard")]
    [InlineData("clay")]
    [InlineData("grass")]
    [InlineData("synthetic")]
    public async Task Every_documented_surface_is_accepted(string surface)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await CreateCourtAsync(v, owner, CourtBody(surface: surface));
    }

    [Theory]
    [InlineData("carpet")]
    [InlineData("Hard")]
    [InlineData("")]
    public async Task An_unknown_surface_is_400_bad_surface_and_nothing_is_created(string surface)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/courts", CourtBody(surface: surface));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        if (surface != "") Assert.Equal("bad_surface", (await ReadAsync(res, HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal(2, (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").GetArrayLength());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A_court_needs_a_name(string name)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/courts", CourtBody(name));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(2, (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").GetArrayLength());
    }

    [Fact]
    public async Task A_court_name_over_60_characters_is_rejected()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/courts", CourtBody(new string('ก', 61)));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Updating_a_court_changes_its_fields()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("ชื่อใหม่", "clay", sort: 9));

        var dto = await ReadAsync(res);
        Assert.Equal("ชื่อใหม่", dto.GetProperty("name").GetString());
        var courts = (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").EnumerateArray().ToList();
        Assert.Equal(v.CourtB, courts[0].GetProperty("id").GetGuid()); // sort order 2 < 9
        Assert.Equal("clay", courts[1].GetProperty("surface").GetString());
        Assert.False(courts[1].GetProperty("indoor").GetBoolean());
    }

    [Fact]
    public async Task Updating_with_a_bad_surface_keeps_the_court_unchanged()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("x", "lava")), HttpStatusCode.BadRequest, "bad_surface");

        var a = (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").EnumerateArray().First();
        Assert.Equal("คอร์ท A", a.GetProperty("name").GetString());
        Assert.Equal("hard", a.GetProperty("surface").GetString());
    }

    [Fact]
    public async Task A_court_of_another_venue_or_an_unknown_court_is_404_and_untouched()
    {
        var mine = await ScheduleVenue.CreateAsync(api);
        var theirs = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(mine);

        Assert.Equal(HttpStatusCode.NotFound, (await owner.Http.PutAsJsonAsync($"{Base(mine)}/courts/{theirs.CourtA}", CourtBody("hijack"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await owner.Http.PutAsJsonAsync($"{Base(mine)}/courts/{Guid.NewGuid()}", CourtBody("ghost"))).StatusCode);

        var other = (await SettingsAsync(theirs.Staff.Http, theirs)).GetProperty("courts").EnumerateArray().First();
        Assert.Equal("คอร์ท A", other.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("held")]
    [InlineData("checked_in")]
    public async Task Deactivating_a_court_with_a_live_future_booking_is_409_court_has_bookings(string status)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: status, payment: null,
            holdExpiresAt: status == "held" ? api.Clock.GetUtcNow().UtcDateTime.AddMinutes(10) : null);

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("คอร์ท A", "hard", active: false)),
            HttpStatusCode.Conflict, "court_has_bookings");

        var a = (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").EnumerateArray().First(c => c.GetProperty("id").GetGuid() == v.CourtA);
        Assert.True(a.GetProperty("isActive").GetBoolean());
    }

    [Theory]
    [InlineData("cancelled")]
    [InlineData("expired")]
    [InlineData("no_show")]
    [InlineData("completed")]
    public async Task Bookings_that_no_longer_hold_the_court_do_not_block_deactivation(string status)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: status, payment: null);

        var res = await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("คอร์ท A", "hard", active: false));

        Assert.False((await ReadAsync(res)).GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task A_booking_that_already_ended_or_is_on_another_court_does_not_block_deactivation()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        await v.AddBookingAsync(v.CourtA, Today.AddDays(-3), ["10:00"]);   // finished days ago
        await v.AddBookingAsync(v.CourtB, Future, ["10:00"]);              // live, but on court B

        var res = await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("คอร์ท A", "hard", active: false));

        Assert.False((await ReadAsync(res)).GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task A_booking_that_ends_later_today_still_blocks_deactivation_but_one_that_ended_earlier_does_not()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        // Insert slots relative to the fake clock so this is time-of-day independent.
        var now = api.Clock.GetUtcNow().UtcDateTime;
        var id = Guid.NewGuid();
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO bookings (id, venue_id, status, source, subtotal, addon_total, total) VALUES ({id}, {v.Id}, 'confirmed', 'walk_in', 400, 0, 400)
                """);
            var s = now.AddMinutes(-30); var e = now.AddMinutes(30); // in progress right now
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO booking_slots (booking_id, court_id, start_at, end_at, price, status) VALUES ({id}, {v.CourtA}, {s}, {e}, 400, 'confirmed')
                """);
        });

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("คอร์ท A", "hard", active: false)),
            HttpStatusCode.Conflict, "court_has_bookings");

        api.Clock.Advance(TimeSpan.FromMinutes(31)); // now it has ended
        var res = await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtA}", CourtBody("คอร์ท A", "hard", active: false));
        Assert.False((await ReadAsync(res)).GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task A_deactivated_court_disappears_from_availability_and_cannot_be_booked_and_can_be_reactivated()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        (await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtB}", CourtBody("คอร์ท B", "clay", active: false))).EnsureSuccessStatusCode();

        await WithActiveAsync(v, async () =>
        {
            var courts = (await AvailabilityAsync(api.Anonymous(), v, Future)).GetProperty("courts").EnumerateArray().ToList();
            Assert.Equal(new[] { v.CourtA }, courts.Select(c => c.GetProperty("courtId").GetGuid()).ToArray());
            var detail = await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"));
            Assert.DoesNotContain(v.CourtB, detail.GetProperty("courts").EnumerateArray().Select(c => c.GetProperty("id").GetGuid()));

            var player = await api.LoginAsync();
            var res = await player.Http.PostAsJsonAsync("/api/v1/bookings",
                new { venueId = v.Id, date = Future.ToString("yyyy-MM-dd"), slots = new[] { new { courtId = v.CourtB, start = "10:00" } } });
            await AssertErrorAsync(res, HttpStatusCode.BadRequest, "court_not_found");

            (await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{v.CourtB}", CourtBody("คอร์ท B", "clay", active: true))).EnsureSuccessStatusCode();
            Assert.Equal(2, (await AvailabilityAsync(api.Anonymous(), v, Future)).GetProperty("courts").GetArrayLength());
        });
    }

    [Fact]
    public async Task There_is_no_court_delete_endpoint()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.DeleteAsync($"{Base(v)}/courts/{v.CourtA}");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, res.StatusCode);
        Assert.Equal(2, (await SettingsAsync(v.Staff.Http, v)).GetProperty("courts").GetArrayLength());
    }

    // ------------------------------------------------------------------ hours

    [Fact]
    public async Task Hours_replace_the_whole_week_and_an_omitted_day_is_closed_for_availability_and_booking()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var monday = Next(DayOfWeek.Monday);
        var tuesday = monday.AddDays(1);

        var res = await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day((int)DayOfWeek.Monday, "08:00", "12:00")));

        var returned = (await ReadAsync(res)).EnumerateArray().ToList();
        Assert.Single(returned);
        Assert.Equal("08:00", returned[0].GetProperty("open").GetString());
        Assert.Single((await SettingsAsync(v.Staff.Http, v)).GetProperty("hours").EnumerateArray());

        await WithActiveAsync(v, async () =>
        {
            var open = (await AvailabilityAsync(api.Anonymous(), v, monday)).GetProperty("courts")[0].GetProperty("slots").EnumerateArray().ToList();
            Assert.Equal(new[] { "08:00", "09:00", "10:00", "11:00" }, open.Select(s => s.GetProperty("start").GetString()).ToArray());

            var closed = (await AvailabilityAsync(api.Anonymous(), v, tuesday)).GetProperty("courts")[0].GetProperty("slots");
            Assert.Equal(0, closed.GetArrayLength());

            var player = await api.LoginAsync();
            Task<HttpResponseMessage> Hold(DateOnly d, string start) => player.Http.PostAsJsonAsync("/api/v1/bookings",
                new { venueId = v.Id, date = d.ToString("yyyy-MM-dd"), slots = new[] { new { courtId = v.CourtA, start } } });
            await AssertErrorAsync(await Hold(tuesday, "10:00"), HttpStatusCode.BadRequest, "closed");
            await AssertErrorAsync(await Hold(monday, "13:00"), HttpStatusCode.BadRequest, "outside_hours");
            var ok = await ReadAsync(await Hold(monday, "10:00"), HttpStatusCode.Created);
            Assert.Equal(400m, ok.GetProperty("total").GetDecimal());
        });
    }

    [Fact]
    public async Task Thirty_minute_slots_change_the_grid_and_halve_the_price_for_new_bookings()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var dow = (int)Future.DayOfWeek;
        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day(dow, "09:00", "11:00", 30)))).EnsureSuccessStatusCode();

        await WithActiveAsync(v, async () =>
        {
            var av = await AvailabilityAsync(api.Anonymous(), v, Future);
            Assert.Equal(30, av.GetProperty("slotMinutes").GetInt32());
            var slots = av.GetProperty("courts")[0].GetProperty("slots").EnumerateArray().ToList();
            Assert.Equal(new[] { "09:00", "09:30", "10:00", "10:30" }, slots.Select(s => s.GetProperty("start").GetString()).ToArray());
            Assert.All(slots, s => Assert.Equal(200m, s.GetProperty("price").GetDecimal()));

            var player = await api.LoginAsync();
            var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings",
                new { venueId = v.Id, date = Future.ToString("yyyy-MM-dd"), slots = new[] { new { courtId = v.CourtA, start = "09:30" } } }),
                HttpStatusCode.Created);
            Assert.Equal(200m, hold.GetProperty("total").GetDecimal());
        });
    }

    [Fact]
    public async Task Empty_hours_close_every_day()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours())).EnsureSuccessStatusCode();

        Assert.Empty((await SettingsAsync(v.Staff.Http, v)).GetProperty("hours").EnumerateArray());
    }

    [Fact]
    public async Task Changing_hours_leaves_existing_bookings_untouched()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var booking = await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);

        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours())).EnsureSuccessStatusCode();

        var list = await ReadAsync(await v.Staff.Http.GetAsync($"{Base(v)}/bookings?date={Future:yyyy-MM-dd}"));
        var b = list.EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == booking);
        Assert.Equal("confirmed", b.GetProperty("status").GetString());
        Assert.Equal("10:00", b.GetProperty("slots")[0].GetProperty("start").GetString());
    }

    public static IEnumerable<object[]> BadHours() =>
    [
        ["bad_day", Hours(Day(7))],
        ["bad_day", Hours(Day(-1))],
        ["duplicate_day", Hours(Day(1), Day(1, "07:00", "20:00"))],
        ["bad_time", Hours(Day(1, "9:00", "18:00"))],
        ["bad_time", Hours(Day(1, "06:00", "25:00"))],
        ["bad_time", Hours(Day(1, "abc", "18:00"))],
        ["bad_range", Hours(Day(1, "10:00", "10:00"))],
        ["bad_range", Hours(Day(1, "12:00", "10:00"))],
        ["bad_range", Hours(Day(1, "06:00", "21:30", 60))],       // 15.5 h is not a whole number of 60-min slots
        ["bad_range", Hours(Day(1, "06:00", "06:45", 30))],
        ["bad_slot_minutes", Hours(Day(1, "06:00", "21:00", 45))],
        ["bad_slot_minutes", Hours(Day(1, "06:00", "21:00", 0))],
        ["bad_slot_minutes", Hours(Day(1, "06:00", "21:00", 90))],
    ];

    [Theory]
    [MemberData(nameof(BadHours))]
    public async Task Invalid_hours_are_400_with_the_documented_code_and_change_nothing(string code, object body)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync(Base(v) + "/hours", body), HttpStatusCode.BadRequest, code);

        Assert.Equal(7, (await SettingsAsync(v.Staff.Http, v)).GetProperty("hours").GetArrayLength());
    }

    [Fact]
    public async Task One_bad_day_rejects_the_whole_request_atomically()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await AssertErrorAsync(await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day(1, "08:00", "12:00"), Day(2, "12:00", "08:00"))),
            HttpStatusCode.BadRequest, "bad_range");

        var hours = (await SettingsAsync(v.Staff.Http, v)).GetProperty("hours").EnumerateArray().ToList();
        Assert.Equal(7, hours.Count);
        Assert.All(hours, h => Assert.Equal("06:00", h.GetProperty("open").GetString()));
    }

    [Fact]
    public async Task More_than_seven_days_or_a_missing_days_list_is_400()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var eight = Hours(Enumerable.Range(0, 8).Select(i => Day(i)).ToArray());
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", eight)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.Http.PutAsync(Base(v) + "/hours", Json("{}"))).StatusCode);
        Assert.Equal(7, (await SettingsAsync(v.Staff.Http, v)).GetProperty("hours").GetArrayLength());
    }

    [Fact]
    public async Task A_close_time_off_the_slot_grid_is_rejected()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        // 06:00-23:59 is 1079 min: not divisible by 60 or 30 → bad_range (a venue that closes at midnight must use 23:30 or a 30-min grid ending 23:30)
        await AssertErrorAsync(await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day(1, "06:00", "23:59"))), HttpStatusCode.BadRequest, "bad_range");
        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day(1, "06:00", "23:00")))).EnsureSuccessStatusCode();
    }

    // ------------------------------------------------------------------ add-ons

    [Fact]
    public async Task Creating_an_addon_lists_it_and_shows_it_in_the_public_venue_detail()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var created = await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody("  ลูกเทนนิส  ", "per_hour", 25.5m)), HttpStatusCode.Created);
        var id = created.GetProperty("id").GetGuid();

        var addon = (await SettingsAsync(v.Staff.Http, v)).GetProperty("addons").EnumerateArray().Single();
        Assert.Equal("ลูกเทนนิส", addon.GetProperty("name").GetString());
        Assert.Equal("per_hour", addon.GetProperty("unit").GetString());
        Assert.Equal(25.5m, addon.GetProperty("price").GetDecimal());
        Assert.True(addon.GetProperty("isActive").GetBoolean());

        await WithActiveAsync(v, async () =>
        {
            var detail = await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"));
            var pub = detail.GetProperty("addons").EnumerateArray().Single();
            Assert.Equal(id, pub.GetProperty("id").GetGuid());
            Assert.Equal(25.5m, pub.GetProperty("price").GetDecimal());
        });
    }

    [Fact]
    public async Task A_new_addon_is_priced_server_side_in_a_booking_per_hour_and_per_booking()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var perHour = (await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody("a", "per_hour", 30)), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var perBooking = (await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody("b", "per_booking", 50)), HttpStatusCode.Created)).GetProperty("id").GetGuid();

        await WithActiveAsync(v, async () =>
        {
            var player = await api.LoginAsync();
            var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings", new
            {
                venueId = v.Id, date = Future.ToString("yyyy-MM-dd"),
                slots = new[] { new { courtId = v.CourtA, start = "10:00" }, new { courtId = v.CourtA, start = "11:00" } },
                addons = new[] { new { addonId = perHour, qty = 1 }, new { addonId = perBooking, qty = 2 } },
            }), HttpStatusCode.Created);
            Assert.Equal(800m + 60m + 100m, hold.GetProperty("total").GetDecimal());
        });
    }

    [Fact]
    public async Task A_deactivated_addon_vanishes_from_the_public_detail_and_cannot_be_booked_but_stays_in_settings()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var id = (await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody()), HttpStatusCode.Created)).GetProperty("id").GetGuid();

        var res = await owner.Http.PutAsJsonAsync($"{Base(v)}/addons/{id}", AddonBody(active: false));

        Assert.False((await ReadAsync(res)).GetProperty("isActive").GetBoolean());
        Assert.False((await SettingsAsync(v.Staff.Http, v)).GetProperty("addons").EnumerateArray().Single().GetProperty("isActive").GetBoolean());
        await WithActiveAsync(v, async () =>
        {
            var detail = await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"));
            Assert.Empty(detail.GetProperty("addons").EnumerateArray());

            var player = await api.LoginAsync();
            await AssertErrorAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings", new
            {
                venueId = v.Id, date = Future.ToString("yyyy-MM-dd"),
                slots = new[] { new { courtId = v.CourtA, start = "10:00" } }, addons = new[] { new { addonId = id, qty = 1 } },
            }), HttpStatusCode.BadRequest, "addon_not_found");

            (await owner.Http.PutAsJsonAsync($"{Base(v)}/addons/{id}", AddonBody(active: true))).EnsureSuccessStatusCode();
            Assert.Single((await ReadAsync(await api.Anonymous().GetAsync($"/api/v1/venues/{v.Id}"))).GetProperty("addons").EnumerateArray());
        });
    }

    [Fact]
    public async Task Updating_an_addon_does_not_change_the_price_of_a_booking_already_made()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var id = (await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody(price: 100)), HttpStatusCode.Created)).GetProperty("id").GetGuid();

        await WithActiveAsync(v, async () =>
        {
            var player = await api.LoginAsync();
            var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings", new
            {
                venueId = v.Id, date = Future.ToString("yyyy-MM-dd"),
                slots = new[] { new { courtId = v.CourtA, start = "10:00" } }, addons = new[] { new { addonId = id, qty = 1 } },
            }), HttpStatusCode.Created);
            (await owner.Http.PutAsJsonAsync($"{Base(v)}/addons/{id}", AddonBody(price: 999))).EnsureSuccessStatusCode();

            var again = await player.Http.GetBookingAsync(hold.Id());
            Assert.Equal(500m, again.GetProperty("total").GetDecimal());
        });
    }

    [Theory]
    [InlineData("hourly")]
    [InlineData("PER_HOUR")]
    [InlineData("")]
    public async Task An_unknown_addon_unit_is_400_bad_unit(string unit)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody(unit: unit));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        if (unit != "") Assert.Equal("bad_unit", (await ReadAsync(res, HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Empty((await SettingsAsync(v.Staff.Http, v)).GetProperty("addons").EnumerateArray());
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(100000.01)]
    [InlineData(1000000)]
    public async Task An_addon_price_outside_0_to_100000_is_400_bad_price(double price)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        await AssertErrorAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody(price: (decimal)price)), HttpStatusCode.BadRequest, "bad_price");
        Assert.Empty((await SettingsAsync(v.Staff.Http, v)).GetProperty("addons").EnumerateArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100000)]
    public async Task Addon_price_boundaries_are_inclusive(double price)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        var res = await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody(price: (decimal)price));

        Assert.Equal((decimal)price, (await ReadAsync(res, HttpStatusCode.Created)).GetProperty("price").GetDecimal());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task An_addon_needs_a_name(string name)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);

        Assert.Equal(HttpStatusCode.BadRequest, (await owner.Http.PostAsJsonAsync(Base(v) + "/addons", AddonBody(name))).StatusCode);
        Assert.Empty((await SettingsAsync(v.Staff.Http, v)).GetProperty("addons").EnumerateArray());
    }

    [Fact]
    public async Task An_addon_of_another_venue_or_unknown_is_404_and_untouched_and_there_is_no_delete()
    {
        var mine = await ScheduleVenue.CreateAsync(api);
        var theirs = await ScheduleVenue.CreateAsync(api);
        var myOwner = await OwnerAsync(mine);
        var theirOwner = await OwnerAsync(theirs);
        var theirAddon = (await ReadAsync(await theirOwner.Http.PostAsJsonAsync(Base(theirs) + "/addons", AddonBody("ของเขา")), HttpStatusCode.Created)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await myOwner.Http.PutAsJsonAsync($"{Base(mine)}/addons/{theirAddon}", AddonBody("hijack"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await myOwner.Http.PutAsJsonAsync($"{Base(mine)}/addons/{Guid.NewGuid()}", AddonBody())).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await theirOwner.Http.DeleteAsync($"{Base(theirs)}/addons/{theirAddon}")).StatusCode);

        Assert.Equal("ของเขา", (await SettingsAsync(theirs.Staff.Http, theirs)).GetProperty("addons")[0].GetProperty("name").GetString());
    }

    // ------------------------------------------------------------------ price preview

    private static JsonElement PreviewCourt(JsonElement p, Guid court) =>
        p.GetProperty("courts").EnumerateArray().Single(c => c.GetProperty("courtId").GetGuid() == court);

    private static JsonElement PreviewSlot(JsonElement p, Guid court, string start) =>
        PreviewCourt(p, court).GetProperty("slots").EnumerateArray().Single(s => s.GetProperty("start").GetString() == start);

    private static async Task<JsonElement> PreviewAsync(HttpClient http, ScheduleVenue v, DateOnly date) =>
        await ReadAsync(await http.GetAsync($"{Base(v)}/prices/preview?date={date:yyyy-MM-dd}"));

    [Fact]
    public async Task Preview_lists_every_slot_per_active_court_with_court_rule_over_venue_rule_and_labels()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var p = await PreviewAsync(v.Staff.Http, v, Future);

        Assert.Equal(Future.ToString("yyyy-MM-dd"), p.GetProperty("date").GetString());
        Assert.Equal(60, p.GetProperty("slotMinutes").GetInt32());
        Assert.Equal(new[] { v.CourtA, v.CourtB }, p.GetProperty("courts").EnumerateArray().Select(c => c.GetProperty("courtId").GetGuid()).ToArray());
        Assert.Equal("คอร์ท A", PreviewCourt(p, v.CourtA).GetProperty("name").GetString());
        Assert.Equal(16, PreviewCourt(p, v.CourtA).GetProperty("slots").GetArrayLength());
        var six = PreviewSlot(p, v.CourtA, "06:00");
        Assert.Equal("07:00", six.GetProperty("end").GetString());
        Assert.Equal(400m, six.GetProperty("price").GetDecimal());
        Assert.Equal("Day", six.GetProperty("label").GetString());
        Assert.Equal(400m, PreviewSlot(p, v.CourtA, "16:00").GetProperty("price").GetDecimal());
        Assert.Equal(600m, PreviewSlot(p, v.CourtA, "17:00").GetProperty("price").GetDecimal());
        Assert.Equal("Evening", PreviewSlot(p, v.CourtA, "21:00").GetProperty("label").GetString());
        Assert.Equal(500m, PreviewSlot(p, v.CourtB, "10:00").GetProperty("price").GetDecimal());
        Assert.Equal("Clay", PreviewSlot(p, v.CourtB, "19:00").GetProperty("label").GetString()); // court rule beats a higher-priority venue rule
    }

    [Fact]
    public async Task Preview_omits_inactive_courts_and_a_closed_day_has_no_slots()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.SetCourtActiveAsync(v.CourtB, false);
        await v.DeleteHoursForAsync(Future.DayOfWeek);

        var p = await PreviewAsync(v.Staff.Http, v, Future);

        var courts = p.GetProperty("courts").EnumerateArray().ToList();
        Assert.Single(courts);
        Assert.Equal(v.CourtA, courts[0].GetProperty("courtId").GetGuid());
        Assert.Empty(courts[0].GetProperty("slots").EnumerateArray());
    }

    [Fact]
    public async Task Preview_marks_a_slot_no_rule_covers_with_a_null_price()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.DeletePriceRulesAsync("Evening");

        var p = await PreviewAsync(v.Staff.Http, v, Future);

        var slot = PreviewSlot(p, v.CourtA, "18:00");
        Assert.Equal(JsonValueKind.Null, slot.GetProperty("price").ValueKind);
        Assert.Equal(JsonValueKind.Null, slot.GetProperty("label").ValueKind);
        Assert.Equal(400m, PreviewSlot(p, v.CourtA, "10:00").GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Preview_uses_the_days_slot_length_and_prorates_the_price()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day((int)Future.DayOfWeek, "09:00", "11:00", 30)))).EnsureSuccessStatusCode();

        var p = await PreviewAsync(v.Staff.Http, v, Future);

        Assert.Equal(30, p.GetProperty("slotMinutes").GetInt32());
        var slots = PreviewCourt(p, v.CourtA).GetProperty("slots").EnumerateArray().ToList();
        Assert.Equal(new[] { "09:00", "09:30", "10:00", "10:30" }, slots.Select(s => s.GetProperty("start").GetString()).ToArray());
        Assert.Equal("09:30", slots[0].GetProperty("end").GetString());
        Assert.All(slots, s => Assert.Equal(200m, s.GetProperty("price").GetDecimal()));
        Assert.All(PreviewCourt(p, v.CourtB).GetProperty("slots").EnumerateArray(), s => Assert.Equal(250m, s.GetProperty("price").GetDecimal()));
    }

    [Fact]
    public async Task A_price_rule_created_through_the_prices_endpoint_shows_up_in_the_preview_only_on_matching_days()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var saturday = Next(DayOfWeek.Saturday);
        var wednesday = Next(DayOfWeek.Wednesday);
        (await owner.Http.PostAsJsonAsync(Base(v) + "/prices", new
        {
            courtId = v.CourtA, dayType = "weekend", start = "06:00", end = "22:00", pricePerHour = 999, label = "Weekend A", priority = 5,
        })).EnsureSuccessStatusCode();

        var sat = await PreviewAsync(v.Staff.Http, v, saturday);
        var wed = await PreviewAsync(v.Staff.Http, v, wednesday);

        Assert.Equal(999m, PreviewSlot(sat, v.CourtA, "10:00").GetProperty("price").GetDecimal());
        Assert.Equal("Weekend A", PreviewSlot(sat, v.CourtA, "20:00").GetProperty("label").GetString());
        Assert.Equal(500m, PreviewSlot(sat, v.CourtB, "10:00").GetProperty("price").GetDecimal()); // other court unaffected
        Assert.Equal(400m, PreviewSlot(wed, v.CourtA, "10:00").GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task A_higher_priority_venue_rule_beats_a_lower_one_and_the_preview_follows_edits_and_deletes()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var created = await ReadAsync(await owner.Http.PostAsJsonAsync(Base(v) + "/prices", new
        {
            dayType = "any", start = "10:00", end = "12:00", pricePerHour = 300, label = "Happy", priority = 5,
        }), HttpStatusCode.Created);
        var id = created.GetProperty("id").GetGuid();

        Assert.Equal(300m, PreviewSlot(await PreviewAsync(v.Staff.Http, v, Future), v.CourtA, "11:00").GetProperty("price").GetDecimal());
        Assert.Equal(400m, PreviewSlot(await PreviewAsync(v.Staff.Http, v, Future), v.CourtA, "12:00").GetProperty("price").GetDecimal()); // end exclusive

        (await owner.Http.PutAsJsonAsync($"{Base(v)}/prices/{id}", new { dayType = "any", start = "10:00", end = "12:00", pricePerHour = 350, label = "Happy", priority = 5 })).EnsureSuccessStatusCode();
        Assert.Equal(350m, PreviewSlot(await PreviewAsync(v.Staff.Http, v, Future), v.CourtA, "10:00").GetProperty("price").GetDecimal());

        Assert.Equal(HttpStatusCode.NoContent, (await owner.Http.DeleteAsync($"{Base(v)}/prices/{id}")).StatusCode);
        Assert.Equal(400m, PreviewSlot(await PreviewAsync(v.Staff.Http, v, Future), v.CourtA, "10:00").GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Preview_equals_what_the_public_availability_and_a_booking_charge()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        (await owner.Http.PostAsJsonAsync(Base(v) + "/prices", new
        {
            courtId = v.CourtA, dayType = "any", start = "12:00", end = "14:00", pricePerHour = 777, label = "Lunch", priority = 1,
        })).EnsureSuccessStatusCode();

        await WithActiveAsync(v, async () =>
        {
            var preview = await PreviewAsync(v.Staff.Http, v, Future);
            var av = await AvailabilityAsync(api.Anonymous(), v, Future);
            foreach (var court in av.GetProperty("courts").EnumerateArray())
                foreach (var s in court.GetProperty("slots").EnumerateArray())
                {
                    var p = PreviewSlot(preview, court.GetProperty("courtId").GetGuid(), s.GetProperty("start").GetString()!);
                    Assert.Equal(s.GetProperty("price").GetDecimal(), p.GetProperty("price").GetDecimal());
                    Assert.Equal(s.GetProperty("label").GetString(), p.GetProperty("label").GetString());
                }

            var player = await api.LoginAsync();
            var hold = await ReadAsync(await player.Http.PostAsJsonAsync("/api/v1/bookings",
                new { venueId = v.Id, date = Future.ToString("yyyy-MM-dd"), slots = new[] { new { courtId = v.CourtA, start = "12:00" } } }), HttpStatusCode.Created);
            Assert.Equal(PreviewSlot(preview, v.CourtA, "12:00").GetProperty("price").GetDecimal(), hold.GetProperty("total").GetDecimal());
            Assert.Equal(777m, hold.GetProperty("total").GetDecimal());
        });
    }

    [Fact]
    public async Task Preview_needs_a_valid_date()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        Assert.Equal(HttpStatusCode.BadRequest, (await v.Staff.Http.GetAsync(Base(v) + "/prices/preview")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await v.Staff.Http.GetAsync(Base(v) + "/prices/preview?date=2030-13-45")).StatusCode);
    }

    // ------------------------------------------------------------------ realtime

    [Fact]
    public async Task Settings_writes_do_not_500()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var owner = await OwnerAsync(v);
        var before = api.ServerErrors.Count;

        var id = await CreateCourtAsync(v, owner);
        (await owner.Http.PutAsJsonAsync($"{Base(v)}/courts/{id}", CourtBody("ใหม่", "clay"))).EnsureSuccessStatusCode();
        (await owner.Http.PutAsJsonAsync(Base(v) + "/hours", Hours(Day(1)))).EnsureSuccessStatusCode();

        Assert.True(api.ServerErrors.Count == before, string.Join("\n", api.ServerErrors));
    }
}
