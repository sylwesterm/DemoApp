using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Daily rates of one currency quoted in the provider's base currency.
/// The base currency itself is represented by <see cref="BaseCurrency"/>, whose rate is 1 on every day.
/// </summary>
public sealed class RateSeries
{
    private readonly Dictionary<DateOnly, decimal>? ratesByDate;

    private RateSeries(Dictionary<DateOnly, decimal>? ratesByDate)
    {
        this.ratesByDate = ratesByDate;
    }

    /// <summary>The provider's base currency: worth exactly 1 on every day.</summary>
    public static RateSeries BaseCurrency { get; } = new(null);

    public bool IsBaseCurrency => ratesByDate is null;

    /// <summary>Days with a published rate; empty for <see cref="BaseCurrency"/>, which has a rate on any day.</summary>
    public IEnumerable<DateOnly> Dates => ratesByDate?.Keys ?? Enumerable.Empty<DateOnly>();

    public static RateSeries From(IEnumerable<ExchangeRate> rates)
    {
        ArgumentNullException.ThrowIfNull(rates);

        // A provider may return the same day twice (e.g. overlapping chunks); the last value wins.
        var byDate = new Dictionary<DateOnly, decimal>();
        foreach (var rate in rates)
        {
            byDate[rate.Date] = rate.Value;
        }

        return new RateSeries(byDate);
    }

    public bool TryGetRate(DateOnly date, out decimal rate)
    {
        if (ratesByDate is null)
        {
            rate = 1m;
            return true;
        }

        return ratesByDate.TryGetValue(date, out rate);
    }
}
