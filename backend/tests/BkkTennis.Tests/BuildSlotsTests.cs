using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Bookings;

namespace BkkTennis.Tests;

public class BuildSlotsTests
{
    private static readonly Guid CourtId = Guid.NewGuid();
    private static readonly DateOnly Day = new(2026, 10, 23);
    // 10:30 Bangkok = 03:30 UTC
    private static readonly DateTime Now = new(2026, 10, 23, 3, 30, 0, DateTimeKind.Utc);

    private static (Venue venue, OperatingHours hours) Fixture()
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            Courts = [new Court { Id = CourtId, Name = "Court 1" }],
            PriceRules =
            [
                new PriceRule { StartTime = new(6, 0), EndTime = new(22, 0), PricePerHour = 450, Label = "Standard" },
            ],
        };
        var hours = new OperatingHours { OpenTime = new(6, 0), CloseTime = new(22, 0), SlotMinutes = 60 };
        return (venue, hours);
    }

    private static List<BookingSlot> Build(string start, bool allowInProgress)
    {
        var (venue, hours) = Fixture();
        return BookingService.BuildSlots(venue, hours, Day, [new SlotRequest(CourtId, start)], Now,
            BookingStatus.Confirmed, allowInProgress);
    }

    [Fact]
    public void Prices_slot_server_side_and_converts_to_utc()
    {
        var slot = Assert.Single(Build("14:00", allowInProgress: false));
        Assert.Equal(450, slot.Price);
        Assert.Equal(new DateTime(2026, 10, 23, 7, 0, 0), slot.StartAt);
        Assert.Equal(BookingStatus.Confirmed, slot.Status);
    }

    [Fact]
    public void Online_booking_rejects_slot_already_started()
    {
        var ex = Assert.Throws<BookingException>(() => Build("10:00", allowInProgress: false));
        Assert.Equal("slot_in_past", ex.Code);
    }

    [Fact]
    public void Walk_in_may_take_the_slot_in_progress_but_not_one_that_ended()
    {
        Assert.Single(Build("10:00", allowInProgress: true));
        var ex = Assert.Throws<BookingException>(() => Build("09:00", allowInProgress: true));
        Assert.Equal("slot_in_past", ex.Code);
    }

    [Theory]
    [InlineData("05:00")]
    [InlineData("22:00")]
    [InlineData("14:30")]
    public void Rejects_slots_outside_hours_or_off_grid(string start)
    {
        var ex = Assert.Throws<BookingException>(() => Build(start, allowInProgress: true));
        Assert.Equal("outside_hours", ex.Code);
    }

    [Fact]
    public void Rejects_unknown_court()
    {
        var (venue, hours) = Fixture();
        var ex = Assert.Throws<BookingException>(() => BookingService.BuildSlots(
            venue, hours, Day, [new SlotRequest(Guid.NewGuid(), "14:00")], Now, BookingStatus.Held, false));
        Assert.Equal("court_not_found", ex.Code);
    }
}
