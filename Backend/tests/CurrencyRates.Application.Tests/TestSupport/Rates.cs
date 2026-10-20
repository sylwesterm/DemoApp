using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Tests.TestSupport;

/// <summary>Short notation for rate lists: <c>Rates.Of(("2026-10-15", 4.25m), ...)</c>.</summary>
internal static class Rates
{
    public static IReadOnlyList<ExchangeRate> Of(params (string Date, decimal Value)[] rates) =>
        rates.Select(rate => new ExchangeRate(DateOnly.Parse(rate.Date, System.Globalization.CultureInfo.InvariantCulture), rate.Value))
            .ToList();

    public static DateOnly Day(string date) => DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture);
}
