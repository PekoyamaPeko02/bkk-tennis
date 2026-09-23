using BkkTennis.Api.Domain;

namespace BkkTennis.Api.Services;

public readonly record struct SlotPrice(decimal Price, string? Label);

/// <summary>Pure pricing logic — kept free of EF so it is easy to unit test.</summary>
public static class Pricing
{
    public static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Price for one slot. A rule applies when it covers the slot start time, matches the
    /// day type and is either venue-wide or for this court. Court-specific rules beat
    /// venue-wide ones; then higher priority wins. Returns null when no rule covers the slot.
    /// </summary>
    public static SlotPrice? PriceFor(
        IEnumerable<PriceRule> rules, Guid courtId, DateOnly date, TimeOnly start, int slotMinutes)
    {
        var weekend = IsWeekend(date);
        var rule = rules
            .Where(r => r.CourtId is null || r.CourtId == courtId)
            .Where(r => r.DayType == "any" || (r.DayType == "weekend") == weekend)
            .Where(r => start >= r.StartTime && start < r.EndTime)
            .OrderByDescending(r => r.CourtId.HasValue)
            .ThenByDescending(r => r.Priority)
            .FirstOrDefault();

        if (rule is null) return null;
        var price = decimal.Round(rule.PricePerHour * slotMinutes / 60m, 2, MidpointRounding.AwayFromZero);
        return new SlotPrice(price, rule.Label);
    }

    public static decimal AddonLineTotal(Addon addon, int qty, decimal totalHours) =>
        addon.Unit == "per_hour"
            ? decimal.Round(addon.Price * qty * totalHours, 2, MidpointRounding.AwayFromZero)
            : addon.Price * qty;

    public static decimal Commission(decimal total, decimal rate) =>
        decimal.Round(total * rate, 2, MidpointRounding.AwayFromZero);

    /// <summary>Full refund when cancelled at least <paramref name="freeHours"/> before the first slot.</summary>
    public static decimal RefundAmount(decimal paid, DateTime firstSlotStartUtc, DateTime nowUtc, int freeHours) =>
        firstSlotStartUtc - nowUtc >= TimeSpan.FromHours(freeHours) ? paid : 0m;
}
