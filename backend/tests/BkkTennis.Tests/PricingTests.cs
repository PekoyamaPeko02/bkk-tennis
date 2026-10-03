using BkkTennis.Api.Domain;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Services;

namespace BkkTennis.Tests;

public class PricingTests
{
    private static readonly Guid Court1 = Guid.NewGuid();
    private static readonly Guid Court2 = Guid.NewGuid();
    private static readonly DateOnly Friday = new(2026, 10, 23);
    private static readonly DateOnly Saturday = new(2026, 10, 24);

    private static PriceRule Rule(string start, string end, decimal price, int priority = 0,
        string dayType = "any", Guid? court = null, string? label = null) => new()
    {
        Id = Guid.NewGuid(), StartTime = TimeOnly.Parse(start), EndTime = TimeOnly.Parse(end),
        PricePerHour = price, Priority = priority, DayType = dayType, CourtId = court, Label = label,
    };

    private static readonly List<PriceRule> RacquetClub =
    [
        Rule("06:00", "17:00", 450, label: "Standard"),
        Rule("17:00", "18:00", 600, 10, label: "Prime Twilight Slot"),
        Rule("18:00", "20:00", 650, 10, label: "Floodlit Peak Hour"),
        Rule("20:00", "22:00", 550, 10, label: "Night Match"),
    ];

    [Theory]
    [InlineData("16:00", 450, "Standard")]
    [InlineData("17:00", 600, "Prime Twilight Slot")]
    [InlineData("18:00", 650, "Floodlit Peak Hour")]
    [InlineData("19:00", 650, "Floodlit Peak Hour")]
    [InlineData("21:00", 550, "Night Match")]
    public void Matches_design_price_table(string start, decimal expected, string label)
    {
        var p = Pricing.PriceFor(RacquetClub, Court1, Friday, TimeOnly.Parse(start), 60);
        Assert.NotNull(p);
        Assert.Equal(expected, p!.Value.Price);
        Assert.Equal(label, p.Value.Label);
    }

    [Fact]
    public void Two_selected_slots_from_design_total_1250()
    {
        var total = new[] { "17:00", "18:00" }
            .Sum(t => Pricing.PriceFor(RacquetClub, Court1, Friday, TimeOnly.Parse(t), 60)!.Value.Price);
        Assert.Equal(1250m, total);
    }

    [Fact]
    public void Returns_null_outside_rules()
    {
        Assert.Null(Pricing.PriceFor(RacquetClub, Court1, Friday, TimeOnly.Parse("22:00"), 60));
        Assert.Null(Pricing.PriceFor(RacquetClub, Court1, Friday, TimeOnly.Parse("05:00"), 60));
    }

    [Fact]
    public void Half_hour_slot_is_half_price()
    {
        var p = Pricing.PriceFor(RacquetClub, Court1, Friday, TimeOnly.Parse("17:30"), 30);
        Assert.Equal(300m, p!.Value.Price);
    }

    [Fact]
    public void Court_specific_rule_beats_venue_rule_even_with_lower_priority()
    {
        var rules = new List<PriceRule>(RacquetClub) { Rule("06:00", "22:00", 900, priority: 0, court: Court2) };
        Assert.Equal(900m, Pricing.PriceFor(rules, Court2, Friday, TimeOnly.Parse("18:00"), 60)!.Value.Price);
        Assert.Equal(650m, Pricing.PriceFor(rules, Court1, Friday, TimeOnly.Parse("18:00"), 60)!.Value.Price);
    }

    [Fact]
    public void Weekend_rule_applies_only_on_weekend()
    {
        var rules = new List<PriceRule>(RacquetClub) { Rule("06:00", "22:00", 700, priority: 50, dayType: "weekend") };
        Assert.Equal(700m, Pricing.PriceFor(rules, Court1, Saturday, TimeOnly.Parse("10:00"), 60)!.Value.Price);
        Assert.Equal(450m, Pricing.PriceFor(rules, Court1, Friday, TimeOnly.Parse("10:00"), 60)!.Value.Price);
    }

    [Fact]
    public void Per_hour_addon_scales_with_hours()
    {
        var knocker = new Addon { Price = 350, Unit = "per_hour" };
        var racket = new Addon { Price = 100, Unit = "per_booking" };
        Assert.Equal(700m, Pricing.AddonLineTotal(knocker, 1, 2m));
        Assert.Equal(100m, Pricing.AddonLineTotal(racket, 1, 2m));
    }

    [Fact]
    public void Commission_is_ten_percent_rounded()
    {
        Assert.Equal(125m, Pricing.Commission(1250m, 0.10m));
        Assert.Equal(33.34m, Pricing.Commission(333.35m, 0.10m));
    }

    [Theory]
    [InlineData(5, 1250)]   // 5h before → free cancel
    [InlineData(4, 1250)]   // exactly 4h → free cancel
    [InlineData(3, 0)]      // inside 4h → no refund
    public void Refund_follows_free_cancel_window(int hoursBefore, decimal expected)
    {
        var now = new DateTime(2026, 10, 24, 5, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expected, Pricing.RefundAmount(1250m, now.AddHours(hoursBefore), now, 4));
    }

    [Theory]
    [InlineData("any", "weekend", 0, 0, true)]          // same court scope + priority, overlapping times
    [InlineData("weekday", "weekend", 0, 0, false)]     // disjoint day types
    [InlineData("any", "any", 0, 10, false)]            // different priority is a deliberate override
    public void Ambiguity_needs_same_scope_priority_daytype_and_time_overlap(
        string dayA, string dayB, int prioA, int prioB, bool expected)
    {
        var a = Rule("06:00", "12:00", 400, prioA, dayA);
        var b = Rule("10:00", "14:00", 500, prioB, dayB);
        Assert.Equal(expected, Pricing.AreAmbiguous(a, b));
    }

    [Fact]
    public void Adjacent_ranges_and_different_court_scopes_are_not_ambiguous()
    {
        Assert.False(Pricing.AreAmbiguous(Rule("06:00", "12:00", 400), Rule("12:00", "14:00", 500)));
        Assert.False(Pricing.AreAmbiguous(Rule("06:00", "12:00", 400), Rule("06:00", "12:00", 500, court: Court1)));
    }
}

public class PromptPayTests
{
    [Fact]
    public void Crc16_matches_ccitt_false_check_value() =>
        Assert.Equal(0x29B1, PromptPay.Crc16("123456789"));

    [Fact]
    public void Payload_for_mobile_with_amount()
    {
        var p = PromptPay.Payload("081-234-5678", 1250m);
        Assert.StartsWith("000201010212", p);                 // dynamic QR
        Assert.Contains("29370016A000000677010111011300668123456785303764", p);
        Assert.Contains("54071250.00", p);
        Assert.Contains("5802TH6304", p);
        Assert.Equal(PromptPay.Crc16(p[..^4]).ToString("X4"), p[^4..]);
    }
}

public class AuthTests
{
    [Theory]
    [InlineData("0812345678", "+66812345678")]
    [InlineData("+66 81 234 5678", "+66812345678")]
    [InlineData("66812345678", "+66812345678")]
    [InlineData("12345", null)]
    public void Normalizes_thai_mobile_numbers(string raw, string? expected) =>
        Assert.Equal(expected, AuthEndpoints.NormalizePhone(raw));
}

public class VenueClockTests
{
    [Fact]
    public void Bangkok_is_utc_plus_7()
    {
        var utc = VenueClock.ToUtc(new DateOnly(2026, 10, 24), new TimeOnly(17, 0), "Asia/Bangkok");
        Assert.Equal(new DateTime(2026, 10, 24, 10, 0, 0, DateTimeKind.Utc), utc);
    }
}
