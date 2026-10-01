using System.Net;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// C4: BookingService.SettleBookingsAsync (the 30 s background settle). App booking still `confirmed` 15 min after the first
/// slot started -> no_show; checked_in (or walk-in) booking -> completed once the last slot ended; everything else untouched.
/// The job itself is removed from the test host; tests call the method directly with the fake clock.
/// The DB is shared and SettleBookingsAsync is global, so each test first moves the clock, drains whatever other tests left
/// due, and only THEN inserts its own bookings: that keeps the returned counts exact.
/// </summary>
[Collection(ApiCollection.Name)]
public class SettleBookingsTests(ApiFactory api)
{
    private DateOnly Day => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime).AddDays(1);

    private static DateTime At(DateOnly d, string t) => ScheduleVenue.Utc(d, t);

    /// <summary>Moves the fake clock (forward only) and settles leftovers from other tests.</summary>
    private async Task MoveToAsync(DateTime utc)
    {
        var target = new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc));
        Assert.True(target >= api.Clock.GetUtcNow(), "test bug: the fake clock cannot move backwards");
        api.Clock.SetUtcNow(target);
        for (var i = 0; i < 20; i++)
            if (await SettleAsync() == (0, 0)) return;
        Assert.Fail("could not drain leftover bookings from other tests");
    }

    private Task<(int NoShows, int Completed)> SettleAsync() =>
        api.WithScope(sp => sp.GetRequiredService<BookingService>().SettleBookingsAsync(CancellationToken.None));

    private async Task<(string Booking, string[] Slots)> StateAsync(Guid id)
    {
        (string, string[]) r = default;
        await api.WithDb(async db =>
        {
            var b = await db.Bookings.AsNoTracking().Include(x => x.Slots).SingleAsync(x => x.Id == id);
            r = (b.Status, b.Slots.OrderBy(s => s.StartAt).Select(s => s.Status).ToArray());
        });
        return r;
    }

    private async Task AssertStateAsync(Guid id, string status)
    {
        var (b, slots) = await StateAsync(id);
        Assert.Equal(status, b);
        Assert.All(slots, s => Assert.Equal(status, s));
    }

    private async Task<JsonElement> ScheduleAsync(ScheduleVenue v, DateOnly date)
    {
        var res = await v.Staff.Http.GetAsync(v.Url(date));
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK, $"{(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement;
    }

    private static string CellStatus(JsonElement schedule, Guid court, string start) =>
        schedule.GetProperty("courts").EnumerateArray().Single(c => c.GetProperty("courtId").GetGuid() == court)
            .GetProperty("slots").EnumerateArray().Single(s => s.GetProperty("start").GetString() == start)
            .GetProperty("status").GetString()!;

    // ------------------------------------------------------------------ auto no-show

    [Fact]
    public async Task App_booking_becomes_no_show_at_exactly_the_grace_minutes_and_is_not_refunded()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(15));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00"], userDisplayName: "P", userPhone: "+66811110001");

        Assert.Equal((1, 0), await SettleAsync());

        await AssertStateAsync(id, "no_show");
        await api.WithDb(async db =>
        {
            var pay = await db.Database.SqlQuery<string>($"SELECT status AS \"Value\" FROM payments WHERE booking_id = {id}").ToListAsync();
            Assert.Equal(new[] { "succeeded" }, pay.ToArray()); // no refund: the venue keeps the money
        });
        Assert.Equal((0, 0), await SettleAsync()); // idempotent
        await AssertStateAsync(id, "no_show");
    }

    [Fact]
    public async Task App_booking_is_left_alone_one_second_before_the_grace_period_ends()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(15).AddSeconds(-1));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00"]);

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "confirmed");

        api.Clock.Advance(TimeSpan.FromSeconds(1)); // now exactly 15 min
        await SettleAsync();
        await AssertStateAsync(id, "no_show");
    }

    [Fact]
    public async Task App_booking_before_its_start_is_untouched()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(-5));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00"]);

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "confirmed");
    }

    [Fact]
    public async Task Multi_slot_booking_grace_counts_from_the_first_slot_and_all_slots_are_released()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(14));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00", "11:00", "12:00"]);

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "confirmed");

        api.Clock.Advance(TimeSpan.FromMinutes(1)); // 10:15: the 2nd slot has not started, yet the whole booking lapses
        await SettleAsync();
        var (booking, slots) = await StateAsync(id);
        Assert.Equal("no_show", booking);
        Assert.Equal(new[] { "no_show", "no_show", "no_show" }, slots);
    }

    [Fact]
    public async Task Multi_slot_booking_is_not_a_no_show_just_because_a_later_slot_started_long_ago()
    {
        // guards against using Max/any-slot instead of the first slot: nothing is due before 10:15
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(10));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00", "11:00"]);
        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "confirmed");
    }

    // ------------------------------------------------------------------ completion

    [Fact]
    public async Task Checked_in_booking_stays_checked_in_while_playing_and_completes_when_the_last_slot_ends()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(90)); // 11:30, mid-play of a 10:00-12:00 booking
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00", "11:00"], status: "checked_in");

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "checked_in");

        api.Clock.SetUtcNow(new DateTimeOffset(DateTime.SpecifyKind(At(d, "12:00").AddSeconds(-1), DateTimeKind.Utc)));
        await SettleAsync();
        await AssertStateAsync(id, "checked_in"); // one second before the end

        api.Clock.Advance(TimeSpan.FromSeconds(1)); // exactly the end
        await SettleAsync();
        await AssertStateAsync(id, "completed");
        Assert.Equal((0, 0), await SettleAsync());
    }

    [Fact]
    public async Task Checked_in_booking_never_becomes_no_show_even_long_after_the_start()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(50));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00"], status: "checked_in");
        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "checked_in");
    }

    [Fact]
    public async Task Walk_in_is_never_a_no_show_and_completes_after_its_last_slot()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(45)); // way past the grace period
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00", "11:00"], source: "walk_in", customerName: "Somchai", payment: null);

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, "confirmed");

        api.Clock.SetUtcNow(new DateTimeOffset(DateTime.SpecifyKind(At(d, "12:00"), DateTimeKind.Utc)));
        await SettleAsync();
        await AssertStateAsync(id, "completed");
    }

    [Fact]
    public async Task One_run_settles_a_mix_and_reports_exact_counts()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "14:00").AddMinutes(30));
        var noShow = await v.AddBookingAsync(v.CourtA, d, ["12:00"]);                                     // 12:00, long overdue
        var noShow2 = await v.AddBookingAsync(v.CourtB, d, ["14:00"]);                                    // 14:00, 30 min in
        var done = await v.AddBookingAsync(v.CourtA, d, ["10:00"], status: "checked_in");                 // ended
        var walkDone = await v.AddBookingAsync(v.CourtB, d, ["12:00"], source: "walk_in", payment: null); // ended
        var playing = await v.AddBookingAsync(v.CourtA, d, ["14:00"], status: "checked_in");              // 14:00-15:00
        var upcoming = await v.AddBookingAsync(v.CourtA, d, ["16:00"]);                                   // not started

        Assert.Equal((2, 2), await SettleAsync());

        await AssertStateAsync(noShow, "no_show");
        await AssertStateAsync(noShow2, "no_show");
        await AssertStateAsync(done, "completed");
        await AssertStateAsync(walkDone, "completed");
        await AssertStateAsync(playing, "checked_in");
        await AssertStateAsync(upcoming, "confirmed");
        Assert.Equal((0, 0), await SettleAsync());
    }

    [Theory]
    [InlineData("held")]
    [InlineData("cancelled")]
    [InlineData("expired")]
    public async Task Held_cancelled_and_expired_bookings_are_untouched(string status)
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddHours(5));
        var id = await v.AddBookingAsync(v.CourtA, d, ["10:00"], status: status, payment: null,
            holdExpiresAt: status == "held" ? api.Clock.GetUtcNow().UtcDateTime.AddMinutes(5) : null);

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(id, status);
    }

    [Fact]
    public async Task Already_finished_bookings_are_not_processed_again()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddHours(5));
        var a = await v.AddBookingAsync(v.CourtA, d, ["10:00"], status: "no_show");
        var b = await v.AddBookingAsync(v.CourtA, d, ["12:00"], status: "completed");

        Assert.Equal((0, 0), await SettleAsync());
        await AssertStateAsync(a, "no_show");
        await AssertStateAsync(b, "completed");
    }

    // ------------------------------------------------------------------ side effects

    [Fact]
    public async Task Schedule_shows_settled_cells_as_no_show_and_completed()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "13:00").AddMinutes(20));
        var noShow = await v.AddBookingAsync(v.CourtA, d, ["10:00", "11:00"]);
        var walkIn = await v.AddBookingAsync(v.CourtB, d, ["10:00"], source: "walk_in", payment: null);
        var playing = await v.AddBookingAsync(v.CourtB, d, ["13:00"], status: "checked_in");
        var before = await ScheduleAsync(v, d);
        Assert.Equal("booked", CellStatus(before, v.CourtA, "10:00"));
        Assert.Equal("booked", CellStatus(before, v.CourtB, "10:00"));

        Assert.Equal((1, 1), await SettleAsync());

        var s = await ScheduleAsync(v, d);
        Assert.Equal("no_show", CellStatus(s, v.CourtA, "10:00"));
        Assert.Equal("no_show", CellStatus(s, v.CourtA, "11:00"));
        Assert.Equal("completed", CellStatus(s, v.CourtB, "10:00"));
        Assert.Equal("checked_in", CellStatus(s, v.CourtB, "13:00"));
        Assert.Equal("past", CellStatus(s, v.CourtA, "12:00")); // nothing there
        var bookings = s.GetProperty("bookings").EnumerateArray().ToDictionary(x => x.GetProperty("id").GetGuid(), x => x.GetProperty("status").GetString());
        Assert.Equal("no_show", bookings[noShow]);
        Assert.Equal("completed", bookings[walkIn]);
        Assert.Equal("checked_in", bookings[playing]);
    }

    [Fact]
    public async Task A_no_show_frees_the_court_so_the_same_range_can_be_resold()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var d = Day;
        await MoveToAsync(At(d, "10:00").AddMinutes(20));
        var noShow = await v.AddBookingAsync(v.CourtA, d, ["10:00"]);
        var other = await v.AddBookingAsync(v.CourtB, d, ["10:00"], source: "walk_in", payment: null);

        // control: while the first booking is live the EXCLUDE constraint rejects an overlapping slot
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => v.AddSlotAsync(other, v.CourtA, d, "10:00"));
        Assert.Contains("23P01", ex.ToString());

        Assert.Equal((1, 0), await SettleAsync());
        await AssertStateAsync(noShow, "no_show");

        await v.AddSlotAsync(other, v.CourtA, d, "10:00"); // now accepted
    }
}
