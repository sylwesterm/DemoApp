using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Domain.Tests.Models;

public class ExchangeRateTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-4.25)]
    public void Constructor_NonPositiveValue_ThrowsDomainException(decimal value)
    {
        Should.Throw<DomainException>(() => new ExchangeRate(new DateOnly(2026, 10, 15), value))
            .Message.ShouldStartWith("Kurs z dnia 2026-10-15 musi być dodatni");
    }

    [Fact]
    public void Constructor_PositiveValue_StoresDateAndValue()
    {
        var rate = new ExchangeRate(new DateOnly(2026, 10, 15), 0.000234m);

        rate.Date.ShouldBe(new DateOnly(2026, 10, 15));
        rate.Value.ShouldBe(0.000234m);
    }
}
