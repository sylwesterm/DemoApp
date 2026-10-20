using AutoMapper;
using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Rates published by Narodowy Bank Polski: average rates from table A (daily, major currencies)
/// and table B (weekly, other currencies), all quoted in PLN.
/// </summary>
public sealed class NbpCurrencyRateProvider(
    INbpApiClient apiClient,
    IMemoryCache cache,
    IOptions<NbpOptions> options,
    IMapper mapper) : ICurrencyRateProvider
{
    public const string ProviderKey = "NBP";

    // Order matters: when a code appears in both tables, table A (the primary one) wins.
    private static readonly string[] Tables = ["A", "B"];

    // NBP quotes everything in PLN, so PLN is not listed in its tables.
    private static readonly Currency Pln = new("PLN", "złoty polski");

    private const string CatalogCacheKey = "nbp:currency-catalog";

    public string Key => ProviderKey;

    public string DisplayName => options.Value.DisplayName;

    public string BaseCurrencyCode => Pln.Code;

    public async Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken) =>
        (await GetCatalogAsync(cancellationToken)).Currencies;

    public async Task<IReadOnlyList<ExchangeRate>> GetRatesAsync(
        string currencyCode,
        DateRange range,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(range);

        var catalog = await GetCatalogAsync(cancellationToken);
        if (!catalog.TableByCode.TryGetValue(currencyCode, out var table))
        {
            return [];
        }

        var chunks = DateRangeChunker.Split(range, options.Value.MaxDaysPerRequest);
        var responses = await Task.WhenAll(
            chunks.Select(chunk => apiClient.GetRatesAsync(table, currencyCode, chunk, cancellationToken)));

        var rates = responses
            .Where(response => response is not null)
            .SelectMany(response => response!.Rates);

        return MapNbpData(() => mapper.Map<List<ExchangeRate>>(rates))
            .OrderBy(rate => rate.Date)
            .ToList();
    }

    private async Task<NbpCurrencyCatalog> GetCatalogAsync(CancellationToken cancellationToken)
    {
        // A failed load throws before anything is cached, so the next request retries it.
        var catalog = await cache.GetOrCreateAsync(CatalogCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = options.Value.CurrencyListCacheDuration;
            return LoadCatalogAsync(cancellationToken);
        });

        return catalog!;
    }

    private async Task<NbpCurrencyCatalog> LoadCatalogAsync(CancellationToken cancellationToken)
    {
        var tables = await Task.WhenAll(Tables.Select(table => apiClient.GetLatestTableAsync(table, cancellationToken)));

        var currencies = new List<Currency> { Pln };
        var tableByCode = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (table, response) in Tables.Zip(tables))
        {
            foreach (var rate in response.Rates)
            {
                var currency = MapNbpData(() => mapper.Map<Currency>(rate));
                if (tableByCode.TryAdd(currency.Code, table))
                {
                    currencies.Add(currency);
                }
            }
        }

        return new NbpCurrencyCatalog(currencies, tableByCode);
    }

    /// <summary>
    /// Invalid data from NBP breaks a domain rule while mapping. That is an upstream fault (HTTP 502),
    /// not a bad request from our user (HTTP 400), so the domain exception is translated.
    /// </summary>
    private static T MapNbpData<T>(Func<T> map)
    {
        try
        {
            return map();
        }
        catch (AutoMapperMappingException exception) when (exception.InnerException is DomainException domainException)
        {
            throw new ExternalServiceException($"Serwis NBP zwrócił nieprawidłowe dane: {domainException.Message}", exception);
        }
    }
}
