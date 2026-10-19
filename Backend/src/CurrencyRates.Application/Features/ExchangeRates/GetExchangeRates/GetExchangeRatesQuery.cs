using MediatR;

namespace CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;

/// <summary>
/// Rates of <see cref="SourceCurrency"/> in <see cref="TargetCurrency"/> for a period, with statistics.
/// Returns <c>null</c> when the provider has no rates in that period (HTTP 204).
/// </summary>
/// <param name="DateFrom">Start of the period; today (in Poland) when not provided.</param>
/// <param name="DateTo">End of the period; today (in Poland) when not provided.</param>
public sealed record GetExchangeRatesQuery(
    string ProviderKey,
    string SourceCurrency,
    string TargetCurrency,
    DateOnly? DateFrom,
    DateOnly? DateTo) : IRequest<ExchangeRateHistoryDto?>
{
    /// <summary>Applies the "missing date means today" rule from the specification.</summary>
    public (DateOnly From, DateOnly To) ResolveDates(DateOnly today) => (DateFrom ?? today, DateTo ?? today);
}
