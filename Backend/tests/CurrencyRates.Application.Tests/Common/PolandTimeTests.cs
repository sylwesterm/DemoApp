using CurrencyRates.Application.Common;
using Microsoft.Extensions.Time.Testing;

namespace CurrencyRates.Application.Tests.Common;

public class PolandTimeTests
{
    [Theory]
    [InlineData("2026-10-18T21:59:00Z", "2026-10-18")] // 23:59 in Poland (CEST, UTC+2)
    [InlineData("2026-10-18T22:30:00Z", "2026-10-19")] // 00:30 in Poland - already the next day
    [InlineData("2026-12-31T23:30:00Z", "2027-01-01")] // 00:30 in Poland (CET, UTC+1)
    [InlineData("2026-12-31T22:30:00Z", "2026-12-31")] // 23:30 in Poland
    public void GetTodayInPoland_UsesPolishTimeZone(string utcNow, string expectedToday)
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse(utcNow, System.Globalization.CultureInfo.InvariantCulture));

        timeProvider.GetTodayInPoland().ShouldBe(DateOnly.Parse(expectedToday, System.Globalization.CultureInfo.InvariantCulture));
    }
}
