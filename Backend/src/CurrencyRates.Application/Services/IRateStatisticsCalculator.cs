using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

public interface IRateStatisticsCalculator
{
    /// <summary>Calculates the minimum, maximum and average of a non-empty set of rates.</summary>
    /// <exception cref="ArgumentException"><paramref name="rates"/> is empty.</exception>
    RateStatistics Calculate(IReadOnlyCollection<ExchangeRate> rates);
}
