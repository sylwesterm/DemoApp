using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Domain.Tests.Models;

public class DateRangeTests
{
    private static readonly DateOnly October15 = new(2026, 10, 15);

    [Fact]
    public void Constructor_FromAfterTo_ThrowsDomainException()
    {
        Should.Throw<DomainException>(() => new DateRange(October15.AddDays(1), October15))
            .Message.ShouldBe("Data początkowa 2026-10-16 jest późniejsza niż data końcowa 2026-10-15.");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(92, 93)]
    public void DayCount_IncludesBothEnds(int daysAfterStart, int expectedDayCount)
    {
        new DateRange(October15, October15.AddDays(daysAfterStart)).DayCount.ShouldBe(expectedDayCount);
    }

    [Fact]
    public void SingleDay_StartsAndEndsOnThatDay()
    {
        var range = DateRange.SingleDay(October15);

        range.From.ShouldBe(October15);
        range.To.ShouldBe(October15);
        range.DayCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void Contains_ChecksInclusiveBounds(int offset, bool expected)
    {
        var range = new DateRange(October15, October15.AddDays(10));

        range.Contains(October15.AddDays(offset)).ShouldBe(expected);
    }
}
