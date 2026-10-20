using CurrencyRates.Application.Services;
using CurrencyRates.Application.Tests.TestSupport;

namespace CurrencyRates.Application.Tests.Services;

public class RateSeriesTests
{
    [Fact]
    public void BaseCurrency_HasRateOfOneOnAnyDay()
    {
        RateSeries.BaseCurrency.TryGetRate(Rates.Day("2002-01-02"), out var rate).ShouldBeTrue();

        rate.ShouldBe(1m);
        RateSeries.BaseCurrency.IsBaseCurrency.ShouldBeTrue();
    }

    [Fact]
    public void From_DuplicateDay_LastValueWins()
    {
        var series = RateSeries.From(Rates.Of(("2026-10-15", 4.25m), ("2026-10-15", 4.30m)));

        series.TryGetRate(Rates.Day("2026-10-15"), out var rate).ShouldBeTrue();
        rate.ShouldBe(4.30m);
        series.Dates.ShouldHaveSingleItem();
    }

    [Fact]
    public void TryGetRate_DayWithoutRate_ReturnsFalse()
    {
        var series = RateSeries.From(Rates.Of(("2026-10-15", 4.25m)));

        series.TryGetRate(Rates.Day("2026-10-16"), out _).ShouldBeFalse();
    }
}
