using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Converts rates quoted in a common base currency into a direct rate: <c>source → target = source / target</c>.
/// For example with PLN as the base: EUR = 4.25 PLN, USD = 3.90 PLN, so 1 EUR = 4.25 / 3.90 USD.
/// </summary>
public sealed class CrossRateCalculator : ICrossRateCalculator
{
    public IReadOnlyList<ExchangeRate> Calculate(RateSeries source, RateSeries target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        return CommonDates(source, target)
            .Order()
            .Select(date => new ExchangeRate(date, RateRounding.Round(RateOn(source, date) / RateOn(target, date))))
            .ToList();
    }

    /// <summary>
    /// Days on which both currencies have a rate. The base currency has a rate on any day,
    /// so it does not limit the result.
    /// </summary>
    private static IEnumerable<DateOnly> CommonDates(RateSeries source, RateSeries target) =>
        (source.IsBaseCurrency, target.IsBaseCurrency) switch
        {
            (true, true) => [],
            (true, false) => target.Dates,
            (false, true) => source.Dates,
            (false, false) => source.Dates.Intersect(target.Dates),
        };

    private static decimal RateOn(RateSeries series, DateOnly date) =>
        series.TryGetRate(date, out var rate)
            ? rate
            : throw new InvalidOperationException($"Brak kursu z dnia {date:yyyy-MM-dd} w serii kursów.");
}
