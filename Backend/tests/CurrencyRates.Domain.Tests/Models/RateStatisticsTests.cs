using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Domain.Tests.Models;

public class RateStatisticsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveMinimum_ThrowsDomainException(decimal minimum)
    {
        Should.Throw<DomainException>(() => new RateStatistics(minimum, 4.3m, 4.2m))
            .Message.ShouldStartWith("Kurs minimalny musi być dodatni");
    }

    [Fact]
    public void Constructor_MinimumGreaterThanMaximum_ThrowsDomainException()
    {
        Should.Throw<DomainException>(() => new RateStatistics(5m, 1m, 3m))
            .Message.ShouldBe("Kurs minimalny 5 jest większy niż kurs maksymalny 1.");
    }

    [Fact]
    public void Constructor_AverageSlightlyOutsideRange_IsAcceptedBecauseOfRounding()
    {
        var statistics = new RateStatistics(1.1m, 1.1m, 1.09999999m);

        statistics.Average.ShouldBe(1.09999999m);
    }

    [Fact]
    public void Constructor_EqualMinimumAndMaximum_IsValid()
    {
        var statistics = new RateStatistics(4.25m, 4.25m, 4.25m);

        statistics.ShouldBe(new RateStatistics(4.25m, 4.25m, 4.25m));
    }
}
