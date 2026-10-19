using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

public interface ICrossRateCalculator
{
    /// <summary>
    /// Calculates how much one unit of the source currency is worth in the target currency on each day
    /// for which both currencies have a rate. Both series must be quoted in the same base currency.
    /// </summary>
    /// <returns>Rates ordered by date; empty when the series have no common day.</returns>
    IReadOnlyList<ExchangeRate> Calculate(RateSeries source, RateSeries target);
}
