namespace CurrencyRates.Application.Common;

/// <summary>
/// Calendar helpers in Polish time. "Today" is the day in Poland (where NBP publishes rates),
/// independent of the server's time zone.
/// </summary>
public static class PolandTime
{
    // IANA id; supported on Windows too since .NET 6 (ICU).
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static DateOnly GetTodayInPoland(this TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var nowInPoland = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), TimeZone);
        return DateOnly.FromDateTime(nowInPoland.DateTime);
    }
}
