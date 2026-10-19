using CurrencyRates.Domain.Exceptions;

namespace CurrencyRates.Domain.Models;

/// <summary>
/// Minimum, maximum and average exchange rate over a period.
/// </summary>
public sealed record RateStatistics
{
    public RateStatistics(decimal minimum, decimal maximum, decimal average)
    {
        if (minimum <= 0)
        {
            throw new DomainException($"Kurs minimalny musi być dodatni, a wynosi {minimum}.");
        }

        if (minimum > maximum)
        {
            throw new DomainException($"Kurs minimalny {minimum} jest większy niż kurs maksymalny {maximum}.");
        }

        // The average is deliberately not checked against minimum/maximum: decimal division
        // can round it marginally outside that range even for valid data.
        Minimum = minimum;
        Maximum = maximum;
        Average = average;
    }

    public decimal Minimum { get; }

    public decimal Maximum { get; }

    public decimal Average { get; }
}
