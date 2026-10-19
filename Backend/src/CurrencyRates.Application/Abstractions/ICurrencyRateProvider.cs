using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Abstractions;

/// <summary>
/// A source of currency exchange rates (e.g. NBP). Each implementation is registered in DI
/// and automatically becomes selectable in the API provider list.
/// </summary>
/// <remarks>
/// Contract every implementation must honour:
/// <list type="bullet">
/// <item>Rates are expressed in <see cref="BaseCurrencyCode"/>; cross rates are computed by the caller.</item>
/// <item>No data for a period is a normal outcome and yields an empty list, never <c>null</c>.</item>
/// <item>Upstream failures are reported as <see cref="Exceptions.ExternalServiceException"/>.</item>
/// </list>
/// </remarks>
public interface ICurrencyRateProvider
{
    /// <summary>Stable identifier used in routes and configuration, e.g. <c>NBP</c>.</summary>
    string Key { get; }

    /// <summary>Human-readable name shown in the provider drop-down.</summary>
    string DisplayName { get; }

    /// <summary>Currency in which all rates returned by this provider are quoted, e.g. <c>PLN</c>.</summary>
    string BaseCurrencyCode { get; }

    /// <summary>Returns all currencies the provider supports, including its base currency.</summary>
    Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns daily rates of <paramref name="currencyCode"/> in <see cref="BaseCurrencyCode"/>
    /// within <paramref name="range"/>. Must not be called for the base currency itself.
    /// </summary>
    Task<IReadOnlyList<ExchangeRate>> GetRatesAsync(
        string currencyCode,
        DateRange range,
        CancellationToken cancellationToken);
}
