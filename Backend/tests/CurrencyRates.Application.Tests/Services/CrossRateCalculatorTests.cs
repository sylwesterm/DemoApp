using CurrencyRates.Application.Services;
using CurrencyRates.Application.Tests.TestSupport;

namespace CurrencyRates.Application.Tests.Services;

public class CrossRateCalculatorTests
{
    private readonly CrossRateCalculator calculator = new();

    [Fact]
    public void Calculate_TwoForeignCurrencies_DividesSourceByTarget()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m)));
        var usd = RateSeries.From(Rates.Of(("2026-10-15", 3.90m)));

        var result = calculator.Calculate(eur, usd);

        result.ShouldHaveSingleItem().Value.ShouldBe(1.08974359m);
    }

    [Fact]
    public void Calculate_DayMissingInOneSeries_SkipsThatDay()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m), ("2026-10-16", 4.26m), ("2026-10-17", 4.24m)));
        var usd = RateSeries.From(Rates.Of(("2026-10-15", 3.90m), ("2026-10-17", 3.88m)));

        var result = calculator.Calculate(eur, usd);

        result.Select(rate => rate.Date).ShouldBe([Rates.Day("2026-10-15"), Rates.Day("2026-10-17")]);
    }

    [Fact]
    public void Calculate_TargetIsBaseCurrency_ReturnsSourceRatesUnchanged()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m), ("2026-10-16", 4.26m)));

        var result = calculator.Calculate(eur, RateSeries.BaseCurrency);

        result.Select(rate => rate.Value).ShouldBe([4.25m, 4.26m]);
    }

    [Fact]
    public void Calculate_SourceIsBaseCurrency_ReturnsInverseRates()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m)));

        var result = calculator.Calculate(RateSeries.BaseCurrency, eur);

        result.ShouldHaveSingleItem().Value.ShouldBe(0.23529412m);
    }

    [Fact]
    public void Calculate_BothBaseCurrency_ReturnsEmpty()
    {
        calculator.Calculate(RateSeries.BaseCurrency, RateSeries.BaseCurrency).ShouldBeEmpty();
    }

    [Fact]
    public void Calculate_NoCommonDays_ReturnsEmpty()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m)));
        var usd = RateSeries.From(Rates.Of(("2026-10-16", 3.90m)));

        calculator.Calculate(eur, usd).ShouldBeEmpty();
    }

    [Fact]
    public void Calculate_UnorderedInput_ReturnsRatesOrderedByDate()
    {
        var eur = RateSeries.From(Rates.Of(("2026-10-17", 4.24m), ("2026-10-15", 4.25m), ("2026-10-16", 4.26m)));

        var result = calculator.Calculate(eur, RateSeries.BaseCurrency);

        result.Select(rate => rate.Date).ShouldBeInOrder();
    }

    [Fact]
    public void Calculate_VerySmallRate_KeepsEightDecimalPlaces()
    {
        var idr = RateSeries.From(Rates.Of(("2026-10-15", 0.000234m)));
        var eur = RateSeries.From(Rates.Of(("2026-10-15", 4.25m)));

        calculator.Calculate(idr, eur).ShouldHaveSingleItem().Value.ShouldBe(0.00005506m);
    }
}
