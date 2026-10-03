using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;

namespace BkkTennis.IntegrationTests;

/// <summary>C2: GET /clubos/venues/{id}/schedule — the day grid with booking/block details and KPIs for the partner portal.</summary>
[Collection(ApiCollection.Name)]
public class ScheduleTests(ApiFactory api)
{
    private static int _phoneSeq = Random.Shared.Next(0, 900);

    /// <summary>users.phone is unique across the shared test DB: vary the middle digits, which masking hides anyway.</summary>
    private static string Middle() => (Interlocked.Increment(ref _phoneSeq) % 1000).ToString("D3");

    /// <summary>+66 8x xxx 5678 for national 08x-xxx-5678.</summary>
    private static string ThaiPhone(string first3, string last4) => "+66" + first3[1..] + Middle() + last4;

    private DateOnly Today => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
    private DateOnly Future => Today.AddDays(5);

    private static async Task<JsonElement> GetAsync(ScheduleVenue v, DateOnly? date)
    {
        var res = await v.Staff.Http.GetAsync(v.Url(date));
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK, $"{(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement;
    }

    private static JsonElement Court(JsonElement schedule, Guid id) =>
        schedule.GetProperty("courts").EnumerateArray().Single(c => c.GetProperty("courtId").GetGuid() == id);

    private static JsonElement Cell(JsonElement schedule, Guid court, string start) =>
        Court(schedule, court).GetProperty("slots").EnumerateArray().Single(s => s.GetProperty("start").GetString() == start);

    private static string Status(JsonElement schedule, Guid court, string start) => Cell(schedule, court, start).GetProperty("status").GetString()!;

    private static JsonElement BookingOf(JsonElement schedule, Guid id) =>
        schedule.GetProperty("bookings").EnumerateArray().Single(b => b.GetProperty("id").GetGuid() == id);

    // ------------------------------------------------------------------ grid

    [Fact]
    public async Task An_empty_day_is_a_full_grid_of_available_priced_slots()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var s = await GetAsync(v, Future);

        Assert.Equal(v.Id, s.GetProperty("venueId").GetGuid());
        Assert.Equal(Future.ToString("yyyy-MM-dd"), s.GetProperty("date").GetString());
        Assert.Equal(60, s.GetProperty("slotMinutes").GetInt32());
        var courts = s.GetProperty("courts").EnumerateArray().ToList();
        Assert.Equal(new[] { v.CourtA, v.CourtB }, courts.Select(c => c.GetProperty("courtId").GetGuid()).ToArray()); // sort order
        Assert.Equal(new[] { "คอร์ท A", "คอร์ท B" }, courts.Select(c => c.GetProperty("name").GetString()).ToArray());
        Assert.Equal("hard", courts[0].GetProperty("surface").GetString());
        Assert.True(courts[0].GetProperty("indoor").GetBoolean());
        Assert.False(courts[1].GetProperty("indoor").GetBoolean());

        foreach (var c in courts)
        {
            var slots = c.GetProperty("slots").EnumerateArray().ToList();
            Assert.Equal(16, slots.Count); // 06:00 … 21:00
            Assert.Equal("06:00", slots[0].GetProperty("start").GetString());
            Assert.Equal("07:00", slots[0].GetProperty("end").GetString());
            Assert.Equal("21:00", slots[^1].GetProperty("start").GetString());
            Assert.Equal("22:00", slots[^1].GetProperty("end").GetString());
            Assert.All(slots, x => Assert.Equal("available", x.GetProperty("status").GetString()));
            Assert.All(slots, x => Assert.Equal(JsonValueKind.Null, x.GetProperty("bookingId").ValueKind));
        }
        Assert.Equal(400m, Cell(s, v.CourtA, "10:00").GetProperty("price").GetDecimal());
        Assert.Equal("Day", Cell(s, v.CourtA, "10:00").GetProperty("label").GetString());
        Assert.Equal(600m, Cell(s, v.CourtA, "17:00").GetProperty("price").GetDecimal());
        Assert.Equal(500m, Cell(s, v.CourtB, "10:00").GetProperty("price").GetDecimal()); // court-specific rule beats the venue rule
        Assert.Empty(s.GetProperty("bookings").EnumerateArray());
        Assert.Empty(s.GetProperty("blocks").EnumerateArray());
    }

    [Fact]
    public async Task Slot_times_are_venue_local_even_though_the_first_slot_is_the_previous_day_in_utc()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        // 06:00 Bangkok = 23:00 UTC the day before: a naive "UTC date" filter would lose this booking.
        var id = await v.AddBookingAsync(v.CourtA, Future, ["06:00"]);

        var s = await GetAsync(v, Future);

        Assert.Equal("booked", Status(s, v.CourtA, "06:00"));
        Assert.Equal(id, Cell(s, v.CourtA, "06:00").GetProperty("bookingId").GetGuid());
        // …and it is not on the previous local day
        var before = await GetAsync(v, Future.AddDays(-1));
        Assert.Empty(before.GetProperty("bookings").EnumerateArray());
    }

    [Fact]
    public async Task Only_active_courts_appear()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.SetCourtActiveAsync(v.CourtB, false);

        var s = await GetAsync(v, Future);

        Assert.Equal(new[] { v.CourtA }, s.GetProperty("courts").EnumerateArray().Select(c => c.GetProperty("courtId").GetGuid()).ToArray());
    }

    [Fact]
    public async Task A_day_without_operating_hours_has_courts_but_no_slots()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.DeleteHoursForAsync(Future.DayOfWeek);

        var s = await GetAsync(v, Future);

        Assert.Equal(2, s.GetProperty("courts").GetArrayLength());
        Assert.All(s.GetProperty("courts").EnumerateArray(), c => Assert.Empty(c.GetProperty("slots").EnumerateArray()));
        Assert.Equal(0, s.GetProperty("kpis").GetProperty("sellableSlots").GetInt32());
        Assert.Equal(0, s.GetProperty("kpis").GetProperty("occupancyPct").GetInt32()); // no divide-by-zero
    }

    [Fact]
    public async Task Without_a_date_the_schedule_is_for_today_at_the_venue()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var s = await GetAsync(v, null);

        Assert.Equal(Today.ToString("yyyy-MM-dd"), s.GetProperty("date").GetString());
    }

    [Fact]
    public async Task A_malformed_date_is_400_bad_request()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var res = await v.Staff.Http.GetAsync(v.Url().Replace("/schedule", "/schedule?date=2030-13-45"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("bad_request", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Past_slots_are_marked_past_but_bookings_in_the_past_keep_their_real_status()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var yesterday = Today.AddDays(-1);
        await v.AddBookingAsync(v.CourtA, yesterday, ["10:00"], status: "completed");

        var s = await GetAsync(v, yesterday);

        Assert.Equal("completed", Status(s, v.CourtA, "10:00"));
        Assert.Equal("past", Status(s, v.CourtA, "11:00"));
        Assert.Equal("past", Status(s, v.CourtB, "06:00"));
    }

    [Fact]
    public async Task Today_switches_from_past_to_available_at_the_current_time()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var s = await GetAsync(v, Today);

        var now = api.Clock.GetUtcNow().UtcDateTime;
        foreach (var cell in Court(s, v.CourtA).GetProperty("slots").EnumerateArray())
            Assert.Equal(cell.GetProperty("startAt").GetDateTime() <= now ? "past" : "available", cell.GetProperty("status").GetString());
    }

    // ------------------------------------------------------------------ statuses

    [Theory]
    [InlineData("held", "held")]
    [InlineData("confirmed", "booked")]
    [InlineData("checked_in", "checked_in")]
    [InlineData("no_show", "no_show")]
    [InlineData("completed", "completed")]
    [InlineData("cancelled", "available")]
    [InlineData("expired", "available")]
    public async Task Booking_status_decides_the_cell_status(string bookingStatus, string cell)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var payment = bookingStatus == "held" ? "pending" : "succeeded";
        var hold = bookingStatus == "held" ? api.Clock.GetUtcNow().UtcDateTime.AddMinutes(10) : (DateTime?)null;
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: bookingStatus, payment: payment, holdExpiresAt: hold);

        var s = await GetAsync(v, Future);

        Assert.Equal(cell, Status(s, v.CourtA, "10:00"));
        if (cell == "available")
        {
            Assert.Empty(s.GetProperty("bookings").EnumerateArray()); // cancelled / expired bookings vanish from the day
            Assert.Equal(JsonValueKind.Null, Cell(s, v.CourtA, "10:00").GetProperty("bookingId").ValueKind);
        }
        else Assert.Equal(id, Cell(s, v.CourtA, "10:00").GetProperty("bookingId").GetGuid());
    }

    [Fact]
    public async Task A_released_no_show_slot_can_be_resold_and_the_new_booking_wins_the_cell()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: "no_show");
        var resold = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: "confirmed", source: "walk_in", customerName: "Late Larry", payment: null);

        var s = await GetAsync(v, Future);

        Assert.Equal("booked", Status(s, v.CourtA, "10:00"));
        Assert.Equal(resold, Cell(s, v.CourtA, "10:00").GetProperty("bookingId").GetGuid());
    }

    [Fact]
    public async Task Blocks_close_their_cells_and_are_listed_with_their_reason()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var block = await v.AddBlockAsync(v.CourtB, Future, "10:00", "12:00", reason: "cleaning", note: "ขัดพื้น");

        var s = await GetAsync(v, Future);

        Assert.Equal("available", Status(s, v.CourtB, "09:00"));
        Assert.Equal("blocked", Status(s, v.CourtB, "10:00"));
        Assert.Equal("blocked", Status(s, v.CourtB, "11:00"));
        Assert.Equal("available", Status(s, v.CourtB, "12:00"));
        Assert.Equal("available", Status(s, v.CourtA, "10:00")); // other court untouched
        Assert.Equal(block, Cell(s, v.CourtB, "10:00").GetProperty("blockId").GetGuid());
        var listed = Assert.Single(s.GetProperty("blocks").EnumerateArray());
        Assert.Equal("cleaning", listed.GetProperty("reason").GetString());
        Assert.Equal("ขัดพื้น", listed.GetProperty("note").GetString());
        Assert.Equal(v.CourtB, listed.GetProperty("courtId").GetGuid());
        Assert.Equal("10:00", listed.GetProperty("start").GetString());
        Assert.Equal("12:00", listed.GetProperty("end").GetString());
    }

    [Fact]
    public async Task A_block_spanning_midnight_still_shows_on_each_day()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBlockAsync(v.CourtA, Future, "20:00", "22:00");
        var overnight = ScheduleVenue.Utc(Future, "21:00");
        await api.WithDb(db => Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlInterpolatedAsync(db.Database, $"""
            INSERT INTO court_blocks (court_id, start_at, end_at, reason) VALUES ({v.CourtA}, {overnight.AddHours(2)}, {overnight.AddHours(12)}, 'event')
            """));

        var day = await GetAsync(v, Future);
        var next = await GetAsync(v, Future.AddDays(1));

        Assert.Equal("blocked", Status(day, v.CourtA, "20:00"));
        Assert.Equal("blocked", Status(next, v.CourtA, "06:00")); // the event runs 23:00 → 09:00 local
        Assert.Equal("blocked", Status(next, v.CourtA, "08:00"));
        Assert.Equal("available", Status(next, v.CourtA, "09:00"));
    }

    // ------------------------------------------------------------------ booking details

    [Fact]
    public async Task An_app_booking_shows_the_customer_with_a_masked_phone_and_its_payment_state()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00", "11:00"], userDisplayName: "สมชาย ก.", userPhone: ThaiPhone("081", "5678"),
            addonTotal: 100, addon: ("เช่าไม้เทนนิส", 1, 100));

        var s = await GetAsync(v, Future);
        var b = BookingOf(s, id);

        Assert.Equal("สมชาย ก.", b.GetProperty("customerName").GetString());
        Assert.Equal("081-***-5678", b.GetProperty("customerPhone").GetString());
        Assert.Equal("app", b.GetProperty("source").GetString());
        Assert.Equal("confirmed", b.GetProperty("status").GetString());
        Assert.Equal("paid", b.GetProperty("paymentState").GetString());
        Assert.Equal(900m, b.GetProperty("total").GetDecimal()); // 2 × 400 + 100 add-on
        Assert.Equal(100m, b.GetProperty("addonTotal").GetDecimal());
        Assert.Matches("^BT[0-9]+$", b.GetProperty("code").GetString()!);
        var slots = b.GetProperty("slots").EnumerateArray().ToList();
        Assert.Equal(new[] { "10:00", "11:00" }, slots.Select(x => x.GetProperty("start").GetString()).ToArray());
        Assert.Equal("คอร์ท A", slots[0].GetProperty("courtName").GetString());
        var addon = Assert.Single(b.GetProperty("addons").EnumerateArray());
        Assert.Equal("เช่าไม้เทนนิส", addon.GetProperty("name").GetString());
        Assert.Equal(1, addon.GetProperty("qty").GetInt32());
        // one booking, two cells pointing at it
        Assert.Equal(id, Cell(s, v.CourtA, "10:00").GetProperty("bookingId").GetGuid());
        Assert.Equal(id, Cell(s, v.CourtA, "11:00").GetProperty("bookingId").GetGuid());
        Assert.Single(s.GetProperty("bookings").EnumerateArray());
    }

    [Fact]
    public async Task The_full_phone_number_never_leaves_the_server()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var phone = ThaiPhone("081", "5678");
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userDisplayName: "Somchai", userPhone: phone);

        var res = await v.Staff.Http.GetAsync(v.Url(Future));
        var raw = await res.Content.ReadAsStringAsync();

        Assert.DoesNotContain(phone, raw);
        Assert.DoesNotContain(phone[3..], raw); // national digits without +66
        Assert.DoesNotContain("+66", raw);
        Assert.DoesNotContain("userId", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("081", "5678", "081-***-5678")]
    [InlineData("098", "4321", "098-***-4321")]
    public async Task Thai_numbers_keep_the_prefix_and_last_four_digits(string first3, string last4, string masked)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userDisplayName: "X", userPhone: ThaiPhone(first3, last4));

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal(masked, b.GetProperty("customerPhone").GetString());
    }

    [Fact]
    public async Task Non_thai_numbers_keep_only_the_last_four_digits()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userDisplayName: "X", userPhone: "+1415" + Middle() + "50123");

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal("***0123", b.GetProperty("customerPhone").GetString());
    }

    [Fact]
    public async Task A_walk_in_shows_its_name_no_phone_and_counter_payment()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtB, Future, ["15:00"], source: "walk_in", customerName: "คุณสมหญิง", payment: null);

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal("walk_in", b.GetProperty("source").GetString());
        Assert.Equal("คุณสมหญิง", b.GetProperty("customerName").GetString());
        Assert.Equal(JsonValueKind.Null, b.GetProperty("customerPhone").ValueKind);
        Assert.Equal("counter", b.GetProperty("paymentState").GetString());
    }

    [Fact]
    public async Task A_booking_without_any_customer_info_has_null_name_and_phone()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtB, Future, ["15:00"], source: "walk_in", payment: null);

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal(JsonValueKind.Null, b.GetProperty("customerName").ValueKind);
        Assert.Equal(JsonValueKind.Null, b.GetProperty("customerPhone").ValueKind);
    }

    [Theory]
    [InlineData("held", "pending", "pending")]
    [InlineData("held", "failed", "failed")]
    [InlineData("confirmed", "succeeded", "paid")]
    [InlineData("cancelled", "refunded", "refunded")]
    [InlineData("confirmed", null, "none")]
    public async Task Payment_state_is_derived_from_the_latest_payment(string bookingStatus, string? payment, string expected)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var hold = bookingStatus == "held" ? api.Clock.GetUtcNow().UtcDateTime.AddMinutes(10) : (DateTime?)null;
        // a cancelled booking is not on the grid, so read the state of the other kinds from the grid and cancelled ones directly
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: bookingStatus == "cancelled" ? "confirmed" : bookingStatus,
            payment: payment, holdExpiresAt: hold);

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal(expected, b.GetProperty("paymentState").GetString());
    }

    [Fact]
    public async Task A_held_booking_exposes_when_the_hold_expires()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var expires = api.Clock.GetUtcNow().UtcDateTime.AddMinutes(7);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: "held", payment: "pending", holdExpiresAt: expires);

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.InRange((b.GetProperty("holdExpiresAt").GetDateTime() - expires).TotalSeconds, -1, 1);
    }

    // ------------------------------------------------------------------ KPIs

    [Fact]
    public async Task Kpis_count_paid_and_walk_in_revenue_but_not_pending_or_cancelled_holds()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        // online, confirmed: 1 h × 400 + 100 add-on = 500
        await v.AddBookingAsync(v.CourtA, Future, ["09:00"], userDisplayName: "A", userPhone: "+66811111111", addonTotal: 100, addon: ("ไม้", 1, 100));
        // online, checked in: 2 h evening × 600 = 1200
        await v.AddBookingAsync(v.CourtA, Future, ["18:00", "19:00"], status: "checked_in", userDisplayName: "B", userPhone: "+66822222222");
        // online no-show: paid, the venue keeps it: 400
        await v.AddBookingAsync(v.CourtA, Future, ["12:00"], status: "no_show", userDisplayName: "C", userPhone: "+66833333333");
        // walk-in on the clay court: 500, no commission side effects here
        await v.AddBookingAsync(v.CourtB, Future, ["10:00"], source: "walk_in", customerName: "Walk", payment: null);
        // unpaid hold: occupies a cell but is neither occupancy nor revenue
        await v.AddBookingAsync(v.CourtB, Future, ["14:00"], status: "held", payment: "pending", userDisplayName: "D", userPhone: "+66844444444",
            holdExpiresAt: api.Clock.GetUtcNow().UtcDateTime.AddMinutes(10));
        // cancelled: invisible
        await v.AddBookingAsync(v.CourtB, Future, ["16:00"], status: "cancelled", userDisplayName: "E", userPhone: "+66855555555");

        var k = (await GetAsync(v, Future)).GetProperty("kpis");

        Assert.Equal(32, k.GetProperty("sellableSlots").GetInt32());
        Assert.Equal(5, k.GetProperty("occupiedSlots").GetInt32()); // 1 + 2 + 1 + 1
        Assert.Equal(1, k.GetProperty("heldSlots").GetInt32());
        Assert.Equal(16, k.GetProperty("occupancyPct").GetInt32()); // 5 / 32 = 15.6 → 16
        Assert.Equal(500m + 1200m + 400m + 500m, k.GetProperty("revenue").GetDecimal());
        Assert.Equal(500m + 1200m + 400m, k.GetProperty("onlineRevenue").GetDecimal());
        Assert.Equal(500m, k.GetProperty("walkInRevenue").GetDecimal());
        Assert.Equal(3, k.GetProperty("onlineBookings").GetInt32());
        Assert.Equal(1, k.GetProperty("walkInBookings").GetInt32());
        Assert.Equal(1, k.GetProperty("pendingBookings").GetInt32());
    }

    [Fact]
    public async Task Blocked_slots_are_not_sellable_capacity()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBlockAsync(v.CourtB, Future, "10:00", "12:00");
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);

        var k = (await GetAsync(v, Future)).GetProperty("kpis");

        Assert.Equal(30, k.GetProperty("sellableSlots").GetInt32()); // 32 − 2 blocked
        Assert.Equal(1, k.GetProperty("occupiedSlots").GetInt32());
        Assert.Equal(3, k.GetProperty("occupancyPct").GetInt32()); // 1/30 = 3.3 → 3
    }

    [Fact]
    public async Task An_empty_day_has_zero_kpis()
    {
        var v = await ScheduleVenue.CreateAsync(api);

        var k = (await GetAsync(v, Future)).GetProperty("kpis");

        foreach (var name in new[] { "occupiedSlots", "heldSlots", "occupancyPct", "onlineBookings", "walkInBookings", "pendingBookings" })
            Assert.Equal(0, k.GetProperty(name).GetInt32());
        Assert.Equal(0m, k.GetProperty("revenue").GetDecimal());
        Assert.Equal(32, k.GetProperty("sellableSlots").GetInt32());
    }

    // ------------------------------------------------------------------ isolation & access

    [Fact]
    public async Task A_venue_never_sees_another_venues_bookings()
    {
        var mine = await ScheduleVenue.CreateAsync(api);
        var other = await ScheduleVenue.CreateAsync(api);
        await other.AddBookingAsync(other.CourtA, Future, ["10:00"], userDisplayName: "Stranger", userPhone: "+66899999999");

        var s = await GetAsync(mine, Future);

        Assert.Empty(s.GetProperty("bookings").EnumerateArray());
        Assert.Equal("available", Status(s, mine.CourtA, "10:00"));
        // and the other venue's staff cannot read mine
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Staff.Http.GetAsync(mine.Url(Future))).StatusCode);
    }

    [Fact]
    public async Task Bookings_on_other_days_are_not_included()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBookingAsync(v.CourtA, Future.AddDays(1), ["10:00"]);
        await v.AddBookingAsync(v.CourtA, Future.AddDays(-1), ["10:00"]);

        var s = await GetAsync(v, Future);

        Assert.Empty(s.GetProperty("bookings").EnumerateArray());
        Assert.Equal(0, s.GetProperty("kpis").GetProperty("occupiedSlots").GetInt32());
    }

    [Fact]
    public async Task The_owner_and_front_desk_both_read_the_same_schedule()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);
        var owner = await api.LoginPartnerAsync(v.Id, memberRole: "owner", role: "partner_owner");

        var a = await v.Staff.Http.GetStringAsync(v.Url(Future));
        var b = await owner.Http.GetStringAsync(v.Url(Future));

        Assert.Equal(a, b);
    }

    [Fact]
    public async Task Anonymous_and_players_cannot_read_a_schedule()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.Anonymous().GetAsync(v.Url(Future))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await player.Http.GetAsync(v.Url(Future))).StatusCode);
    }

    // ------------------------------------------------------------------ priority between live bookings, blocks and finished bookings

    [Fact]
    public async Task A_live_booking_under_a_block_keeps_showing_as_the_booking_not_as_blocked()
    {
        // the API refuses to block over live bookings (409 block_conflict), but the data can still get there (blocks created
        // first, or admin SQL): the portal must keep showing the customer rather than hide a paid booking behind a block.
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);
        await v.AddBlockAsync(v.CourtA, Future, "10:00", "12:00");

        var s = await GetAsync(v, Future);

        Assert.Equal("booked", Status(s, v.CourtA, "10:00"));
        Assert.Equal(id, Cell(s, v.CourtA, "10:00").GetProperty("bookingId").GetGuid());
        Assert.Equal(JsonValueKind.Null, Cell(s, v.CourtA, "10:00").GetProperty("blockId").ValueKind);
        Assert.Equal("blocked", Status(s, v.CourtA, "11:00"));
    }

    [Fact]
    public async Task A_block_over_a_finished_booking_wins_the_cell_but_the_booking_is_still_listed()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], status: "no_show");
        var block = await v.AddBlockAsync(v.CourtA, Future, "10:00", "11:00");

        var s = await GetAsync(v, Future);

        Assert.Equal("blocked", Status(s, v.CourtA, "10:00")); // the slot was released, the court is now closed
        Assert.Equal(block, Cell(s, v.CourtA, "10:00").GetProperty("blockId").GetGuid());
        Assert.Equal("no_show", BookingOf(s, id).GetProperty("status").GetString());
    }

    // ------------------------------------------------------------------ capacity

    [Fact]
    public async Task Unpriced_cells_are_closed_and_not_capacity_unless_something_was_sold_there()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.DeletePriceRulesAsync("Evening"); // court A has no price for 17:00-22:00 (5 cells); court B has its own all-day rule
        var before = await GetAsync(v, Future);
        Assert.Equal("closed", Status(before, v.CourtA, "18:00"));
        Assert.Equal(JsonValueKind.Null, Cell(before, v.CourtA, "18:00").GetProperty("price").ValueKind);
        Assert.Equal(27, before.GetProperty("kpis").GetProperty("sellableSlots").GetInt32()); // 32 - 5

        await v.AddBookingAsync(v.CourtA, Future, ["18:00"]); // sold before the rule was removed

        var after = await GetAsync(v, Future);
        Assert.Equal("booked", Status(after, v.CourtA, "18:00"));
        Assert.Equal(28, after.GetProperty("kpis").GetProperty("sellableSlots").GetInt32());
        Assert.Equal(1, after.GetProperty("kpis").GetProperty("occupiedSlots").GetInt32());
    }

    [Fact]
    public async Task At_the_exact_start_of_a_slot_the_empty_cell_is_already_past()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        // jump the (forward-only) fake clock to tomorrow 10:00:00 venue time, then log in again: tokens are validated on the fake clock
        var tomorrow = VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime).AddDays(1);
        api.Clock.SetUtcNow(new DateTimeOffset(ScheduleVenue.Utc(tomorrow, "10:00"), TimeSpan.Zero));
        var staff = await api.LoginPartnerAsync(v.Id);

        var res = await staff.Http.GetAsync(v.Url(tomorrow));
        var s = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("past", Status(s, v.CourtA, "09:00"));
        Assert.Equal("past", Status(s, v.CourtA, "10:00")); // start == now: the slot has begun
        Assert.Equal("available", Status(s, v.CourtA, "11:00"));
    }

    // ------------------------------------------------------------------ payments & customers

    [Fact]
    public async Task With_several_payments_the_latest_one_decides_the_payment_state()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var t = api.Clock.GetUtcNow().UtcDateTime.AddHours(-3);
        // a failed attempt followed by a successful one
        var retried = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], payment: null);
        await v.AddPaymentAsync(retried, "failed", t);
        await v.AddPaymentAsync(retried, "succeeded", t.AddMinutes(5));
        // a successful payment later refunded
        var refunded = await v.AddBookingAsync(v.CourtA, Future, ["11:00"], payment: null);
        await v.AddPaymentAsync(refunded, "succeeded", t.AddMinutes(1));
        await v.AddPaymentAsync(refunded, "refunded", t.AddMinutes(30));
        // inserted in the opposite order: creation order in the table must not matter
        var reversed = await v.AddBookingAsync(v.CourtA, Future, ["12:00"], payment: null);
        await v.AddPaymentAsync(reversed, "succeeded", t.AddMinutes(30));
        await v.AddPaymentAsync(reversed, "failed", t.AddMinutes(1));

        var s = await GetAsync(v, Future);

        Assert.Equal("paid", BookingOf(s, retried).GetProperty("paymentState").GetString());
        Assert.Equal("refunded", BookingOf(s, refunded).GetProperty("paymentState").GetString());
        Assert.Equal("paid", BookingOf(s, reversed).GetProperty("paymentState").GetString());
    }

    [Fact]
    public async Task A_customer_with_only_a_phone_has_a_masked_phone_and_no_name()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userPhone: ThaiPhone("089", "0001"));

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal(JsonValueKind.Null, b.GetProperty("customerName").ValueKind);
        Assert.Equal("089-***-0001", b.GetProperty("customerPhone").GetString());
    }

    [Fact]
    public async Task A_typed_customer_name_beats_the_account_name()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], customerName: "ชื่อที่หน้าเคาน์เตอร์", userDisplayName: "Account Name");

        var b = BookingOf(await GetAsync(v, Future), id);

        Assert.Equal("ชื่อที่หน้าเคาน์เตอร์", b.GetProperty("customerName").GetString());
    }

    // ------------------------------------------------------------------ odd shapes of data

    [Fact]
    public async Task A_booking_spanning_two_days_shows_on_both_days_with_all_of_its_slots()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtA, Future, ["20:00"]);
        await v.AddSlotAsync(id, v.CourtA, Future.AddDays(1), "10:00");

        var day1 = await GetAsync(v, Future);
        var day2 = await GetAsync(v, Future.AddDays(1));

        Assert.Equal(id, Cell(day1, v.CourtA, "20:00").GetProperty("bookingId").GetGuid());
        Assert.Equal("available", Status(day1, v.CourtA, "10:00")); // day 2's slot does not leak into day 1's grid
        Assert.Equal(id, Cell(day2, v.CourtA, "10:00").GetProperty("bookingId").GetGuid());
        Assert.Equal("available", Status(day2, v.CourtA, "20:00"));
        foreach (var day in new[] { day1, day2 })
        {
            var b = BookingOf(day, id);
            Assert.Equal(2, b.GetProperty("slots").GetArrayLength());
            Assert.Equal(1, day.GetProperty("kpis").GetProperty("occupiedSlots").GetInt32());
        }
    }

    [Fact]
    public async Task A_booking_on_a_deactivated_court_is_still_listed_but_has_no_cell()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var id = await v.AddBookingAsync(v.CourtB, Future, ["10:00"]);
        await v.SetCourtActiveAsync(v.CourtB, false);

        var s = await GetAsync(v, Future);

        Assert.Equal(new[] { v.CourtA }, s.GetProperty("courts").EnumerateArray().Select(c => c.GetProperty("courtId").GetGuid()).ToArray());
        Assert.Equal("paid", BookingOf(s, id).GetProperty("paymentState").GetString()); // the customer is not lost from the list
        var slot = Assert.Single(BookingOf(s, id).GetProperty("slots").EnumerateArray());
        Assert.Equal("คอร์ท B", slot.GetProperty("courtName").GetString()); // the court's name survives deactivation
    }

    // ------------------------------------------------------------------ performance

    [Fact]
    public async Task The_number_of_sql_commands_does_not_grow_with_the_number_of_bookings()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBookingAsync(v.CourtA, Future, ["06:00"], userDisplayName: "First", userPhone: ThaiPhone("081", "1111"));
        await v.AddBookingAsync(v.CourtA, Future, ["07:00"], userDisplayName: "Second", userPhone: ThaiPhone("081", "2222"), addonTotal: 50, addon: ("ไม้", 1, 50));
        await GetAsync(v, Future); // warm up: first request compiles queries
        var small = api.SqlCommands;
        await GetAsync(v, Future);
        var smallCount = api.SqlCommands - small;

        for (var h = 8; h < 22; h++) // 14 more on A + 16 on B = 32 bookings in total, each with its own user and payment
        {
            await v.AddBookingAsync(v.CourtA, Future, [$"{h:D2}:00"], userDisplayName: $"A{h}", userPhone: ThaiPhone("082", h.ToString("D4")),
                addonTotal: 50, addon: ("ไม้", 1, 50));
            await v.AddBookingAsync(v.CourtB, Future, [$"{h - 2:D2}:00"], userDisplayName: $"B{h}", userPhone: ThaiPhone("083", h.ToString("D4")));
        }
        var before = api.SqlCommands;
        var s = await GetAsync(v, Future);
        var bigCount = api.SqlCommands - before;

        Assert.True(s.GetProperty("bookings").GetArrayLength() >= 30);
        Assert.True(smallCount > 0, "the SQL command counter is not counting");
        Assert.Equal(smallCount, bigCount);
    }

    // ------------------------------------------------------------------ contract with packages/api/src/types.ts

    private sealed record TsField(string Name, string Type);

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "CLAUDE.md"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }

    /// <summary>Top-level fields (own + inherited through <c>extends Omit&lt;Parent, …&gt;</c>) of an exported TS interface; own fields win.</summary>
    private static Dictionary<string, string> TsInterface(string ts, string name)
    {
        var m = System.Text.RegularExpressions.Regex.Match(ts,
            @"export interface " + name + @"(?: extends (?<ext>[^{]+?))? \{\r?\n(?<body>.*?)^\}", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Multiline);
        Assert.True(m.Success, $"interface {name} not found in types.ts");
        var fields = new Dictionary<string, string>();
        if (m.Groups["ext"].Success)
        {
            var parent = System.Text.RegularExpressions.Regex.Match(m.Groups["ext"].Value, @"^(?:Omit<)?(\w+)").Groups[1].Value;
            foreach (var (k, t) in TsInterface(ts, parent)) fields[k] = t;
        }
        foreach (System.Text.RegularExpressions.Match f in System.Text.RegularExpressions.Regex.Matches(m.Groups["body"].Value, @"^  (\w+)(\?)?:\s*(.+?);", System.Text.RegularExpressions.RegexOptions.Multiline))
            fields[f.Groups[1].Value] = f.Groups[2].Success ? f.Groups[3].Value + " | undefined" : f.Groups[3].Value;
        return fields;
    }

    private static void AssertMatchesTs(JsonElement obj, Dictionary<string, string> ts, string what)
    {
        var json = obj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        Assert.True(json.Keys.OrderBy(k => k).SequenceEqual(ts.Keys.OrderBy(k => k)),
            $"{what}: JSON keys [{string.Join(",", json.Keys.OrderBy(k => k))}] != TS keys [{string.Join(",", ts.Keys.OrderBy(k => k))}]");
        foreach (var (key, tsType) in ts)
        {
            var kind = json[key].ValueKind;
            var nullable = tsType.Contains("| null");
            if (kind == JsonValueKind.Null) { Assert.True(nullable, $"{what}.{key} is null in JSON but TS says '{tsType}'"); continue; }
            var bare = tsType.Replace("| null", "").Trim();
            if (bare == "number") Assert.True(kind == JsonValueKind.Number, $"{what}.{key}: TS number, JSON {kind}");
            else if (bare == "boolean") Assert.True(kind is JsonValueKind.True or JsonValueKind.False, $"{what}.{key}: TS boolean, JSON {kind}");
            else if (bare == "string" || bare.StartsWith("'")) Assert.True(kind == JsonValueKind.String, $"{what}.{key}: TS {bare}, JSON {kind}");
            else if (bare.EndsWith("[]")) Assert.True(kind == JsonValueKind.Array, $"{what}.{key}: TS {bare}, JSON {kind}");
        }
    }

    [Fact]
    public async Task The_json_matches_the_typescript_types_field_for_field()
    {
        var ts = await File.ReadAllTextAsync(Path.Combine(RepoRoot(), "packages", "api", "src", "types.ts"));
        var v = await ScheduleVenue.CreateAsync(api);
        var app = await v.AddBookingAsync(v.CourtA, Future, ["10:00"], userDisplayName: "สมชาย", userPhone: ThaiPhone("081", "5678"), addonTotal: 100, addon: ("ไม้", 1, 100));
        await v.AddBookingAsync(v.CourtA, Future, ["12:00"], status: "held", payment: "pending", userPhone: ThaiPhone("081", "9999"),
            holdExpiresAt: api.Clock.GetUtcNow().UtcDateTime.AddMinutes(5));
        await v.AddBookingAsync(v.CourtB, Future, ["10:00"], source: "walk_in", payment: null); // null name / phone / hold
        await v.AddBlockAsync(v.CourtB, Future, "14:00", "16:00", note: null);

        var s = await GetAsync(v, Future);

        AssertMatchesTs(s, TsInterface(ts, "Schedule"), "Schedule");
        AssertMatchesTs(s.GetProperty("kpis"), TsInterface(ts, "ScheduleKpis"), "kpis");
        foreach (var c in s.GetProperty("courts").EnumerateArray())
        {
            AssertMatchesTs(c, TsInterface(ts, "ScheduleCourt"), "court");
            foreach (var slot in c.GetProperty("slots").EnumerateArray()) AssertMatchesTs(slot, TsInterface(ts, "ScheduleSlot"), $"slot {slot.GetProperty("start")} {slot.GetProperty("status")}");
        }
        foreach (var b in s.GetProperty("bookings").EnumerateArray())
        {
            AssertMatchesTs(b, TsInterface(ts, "ScheduleBooking"), "booking");
            // inline object types of `slots` / `addons`
            foreach (var slot in b.GetProperty("slots").EnumerateArray())
                AssertInline(ts, "ScheduleBooking", "slots", slot);
            foreach (var addon in b.GetProperty("addons").EnumerateArray())
                AssertInline(ts, "ScheduleBooking", "addons", addon);
        }
        foreach (var b in s.GetProperty("blocks").EnumerateArray()) AssertMatchesTs(b, TsInterface(ts, "ScheduleBlock"), "block");
        Assert.NotEmpty(s.GetProperty("blocks").EnumerateArray());
        Assert.NotEmpty(BookingOf(s, app).GetProperty("addons").EnumerateArray());

        // the string unions of the TS types cover every value the server produced
        var statuses = System.Text.RegularExpressions.Regex.Match(ts, @"export type ScheduleSlotStatus =(.*?);", System.Text.RegularExpressions.RegexOptions.Singleline).Groups[1].Value;
        foreach (var c in s.GetProperty("courts").EnumerateArray())
            foreach (var slot in c.GetProperty("slots").EnumerateArray())
                Assert.Contains($"'{slot.GetProperty("status").GetString()}'", statuses);
        var payments = System.Text.RegularExpressions.Regex.Match(ts, @"export type PaymentState =(.*?);", System.Text.RegularExpressions.RegexOptions.Singleline).Groups[1].Value;
        foreach (var b in s.GetProperty("bookings").EnumerateArray())
            Assert.Contains($"'{b.GetProperty("paymentState").GetString()}'", payments);
    }

    private static void AssertInline(string ts, string iface, string field, JsonElement item)
    {
        var line = System.Text.RegularExpressions.Regex.Match(TsInterfaceRaw(ts, iface), @"^  " + field + @":\s*\{(.*?)\}\[\];", System.Text.RegularExpressions.RegexOptions.Multiline).Groups[1].Value;
        var expected = System.Text.RegularExpressions.Regex.Matches(line, @"(\w+)\??:\s*(\w+)").ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        Assert.True(expected.Count > 0, $"inline type of {iface}.{field} not found");
        AssertMatchesTs(item, expected, $"{iface}.{field}[]");
    }

    private static string TsInterfaceRaw(string ts, string name) =>
        System.Text.RegularExpressions.Regex.Match(ts, @"export interface " + name + @"[^{]*\{\r?\n(.*?)^\}",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Multiline).Groups[1].Value;
}
