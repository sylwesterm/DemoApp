using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp.Models;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Thin wrapper over the NBP Web API (https://api.nbp.pl). Only handles HTTP and JSON;
/// all failures are reported as <see cref="Application.Exceptions.ExternalServiceException"/>.
/// </summary>
public interface INbpApiClient
{
    /// <summary>Returns the latest table of average rates (<c>A</c> - daily, <c>B</c> - weekly).</summary>
    Task<NbpTableDto> GetLatestTableAsync(string table, CancellationToken cancellationToken);

    /// <summary>
    /// Returns rates of a currency in the range, or <c>null</c> when NBP has no data for it (HTTP 404).
    /// The range must not exceed the NBP per-request limit.
    /// </summary>
    Task<NbpRateSeriesDto?> GetRatesAsync(string table, string currencyCode, DateRange range, CancellationToken cancellationToken);
}
