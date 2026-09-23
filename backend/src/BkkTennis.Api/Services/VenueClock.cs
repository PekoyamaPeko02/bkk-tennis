namespace BkkTennis.Api.Services;

/// <summary>Converts between a venue's local wall-clock time and UTC.</summary>
public static class VenueClock
{
    public static TimeZoneInfo Zone(string tz)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(tz); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.CreateCustomTimeZone(tz, TimeSpan.FromHours(7), tz, tz); }
    }

    public static DateTime ToUtc(DateOnly date, TimeOnly time, string tz)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, Zone(tz));
    }

    public static DateTime ToLocal(DateTime utc, string tz) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone(tz));

    public static DateOnly Today(string tz, DateTime? nowUtc = null) =>
        DateOnly.FromDateTime(ToLocal(nowUtc ?? DateTime.UtcNow, tz));
}
