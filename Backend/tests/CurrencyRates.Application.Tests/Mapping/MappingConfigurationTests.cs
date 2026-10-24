using CurrencyRates.Application.Features.Currencies;
using CurrencyRates.Application.Features.ExchangeRates;
using CurrencyRates.Application.Tests.TestSupport;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Tests.Mapping;

public class MappingConfigurationTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        // Fails when a DTO gains a property that no profile maps.
        TestServices.CreateMapper().ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Currency_MapsLabelAsCodeDashName()
    {
        var dto = TestServices.CreateMapper().Map<CurrencyDto>(new Currency("cad", "dolar kanadyjski"));

        dto.ShouldBe(new CurrencyDto { Code = "CAD", Name = "dolar kanadyjski", Label = "CAD - dolar kanadyjski" });
    }

    [Theory]
    [InlineData("yyyy-MM-dd", "2026-10-05")]
    [InlineData("dd.MM.yyyy", "05.10.2026")]
    [InlineData("d MMMM yyyy", "5 października 2026")]
    // "/" is a culture placeholder in .NET (in pl-PL it would print "."); the configured format must be kept as is.
    [InlineData("dd/MM/yyyy", "05/10/2026")]
    public void ExchangeRate_FormatsDateWithConfiguredFormatAndPolishCulture(string format, string expected)
    {
        var dto = TestServices.CreateMapper(format).Map<ExchangeRateDto>(new ExchangeRate(new DateOnly(2026, 10, 5), 4.25m));

        dto.FormattedDate.ShouldBe(expected);
        dto.Date.ShouldBe(new DateOnly(2026, 10, 5));
        dto.Value.ShouldBe(4.25m);
    }
}
