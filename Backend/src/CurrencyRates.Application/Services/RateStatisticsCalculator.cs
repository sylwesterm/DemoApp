using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

public sealed class RateStatisticsCalculator : IRateStatisticsCalculator
{
    public RateStatistics Calculate(IReadOnlyCollection<ExchangeRate> rates)
    {
        ArgumentNullException.ThrowIfNull(rates);

        // "No data" is handled by the caller (HTTP 204), so an empty set here is a programming error.
        if (rates.Count == 0)
        {
            throw new ArgumentException("Nie można obliczyć statystyk dla pustej listy kursów.", nameof(rates));
        }

        var values = rates.Select(rate => rate.Value).ToList();

        return new RateStatistics(
            minimum: values.Min(),
            maximum: values.Max(),
            average: RateRounding.Round(values.Sum() / values.Count));
    }
}
