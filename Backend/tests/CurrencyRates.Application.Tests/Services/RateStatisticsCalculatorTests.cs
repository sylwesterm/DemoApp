using CurrencyRates.Application.Services;
using CurrencyRates.Application.Tests.TestSupport;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Tests.Services;

public class RateStatisticsCalculatorTests
{
    private readonly RateStatisticsCalculator calculator = new();

    [Fact]
    public void Calculate_SeveralRates_ReturnsMinimumMaximumAndAverage()
    {
        var rates = Rates.Of(("2026-10-15", 4.25m), ("2026-10-16", 4.26m), ("2026-10-17", 4.24m));

        calculator.Calculate(rates).ShouldBe(new RateStatistics(4.24m, 4.26m, 4.25m));
    }

    [Fact]
    public void Calculate_SingleRate_AllStatisticsEqualThatRate()
    {
        calculator.Calculate(Rates.Of(("2026-10-15", 4.25m))).ShouldBe(new RateStatistics(4.25m, 4.25m, 4.25m));
    }

    [Fact]
    public void Calculate_RepeatingDecimalAverage_IsRoundedToEightPlaces()
    {
        var rates = Rates.Of(("2026-10-15", 1m), ("2026-10-16", 1m), ("2026-10-17", 2m));

        calculator.Calculate(rates).Average.ShouldBe(1.33333333m);
    }

    [Fact]
    public void Calculate_EmptyList_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => calculator.Calculate([]))
            .Message.ShouldStartWith("Nie można obliczyć statystyk dla pustej listy kursów.");
    }
}
