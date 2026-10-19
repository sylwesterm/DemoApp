using AutoMapper;
using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Common;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Application.Services;
using CurrencyRates.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;

public sealed partial class GetExchangeRatesQueryHandler(
    ICurrencyProviderFactory providerFactory,
    ICrossRateCalculator crossRateCalculator,
    IRateStatisticsCalculator statisticsCalculator,
    TimeProvider timeProvider,
    IMapper mapper,
    ILogger<GetExchangeRatesQueryHandler> logger)
    : IRequestHandler<GetExchangeRatesQuery, ExchangeRateHistoryDto?>
{
    public async Task<ExchangeRateHistoryDto?> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = providerFactory.GetByKey(request.ProviderKey);
        var sourceCode = Currency.NormalizeCode(request.SourceCurrency);
        var targetCode = Currency.NormalizeCode(request.TargetCurrency);
        await EnsureCurrenciesSupportedAsync(provider, [sourceCode, targetCode], cancellationToken);

        var (from, to) = request.ResolveDates(timeProvider.GetTodayInPoland());
        var range = new DateRange(from, to);

        var sourceSeries = LoadSeriesAsync(provider, sourceCode, range, cancellationToken);
        var targetSeries = LoadSeriesAsync(provider, targetCode, range, cancellationToken);
        await Task.WhenAll(sourceSeries, targetSeries);

        var rates = crossRateCalculator.Calculate(await sourceSeries, await targetSeries);
        if (rates.Count == 0)
        {
            LogNoData(provider.Key, sourceCode, targetCode, from, to);
            return null;
        }

        return new ExchangeRateHistoryDto
        {
            Provider = provider.Key,
            SourceCurrency = sourceCode,
            TargetCurrency = targetCode,
            DateFrom = from,
            DateTo = to,
            Statistics = mapper.Map<RateStatisticsDto>(statisticsCalculator.Calculate(rates)),
            Rates = mapper.Map<List<ExchangeRateDto>>(rates),
        };
    }

    private static async Task EnsureCurrenciesSupportedAsync(
        ICurrencyRateProvider provider,
        IEnumerable<string> codes,
        CancellationToken cancellationToken)
    {
        var supported = (await provider.GetCurrenciesAsync(cancellationToken))
            .Select(currency => currency.Code)
            .ToHashSet(StringComparer.Ordinal);

        var unsupported = codes.FirstOrDefault(code => !supported.Contains(code));
        if (unsupported is not null)
        {
            throw new NotFoundException($"Waluta '{unsupported}' nie jest obsługiwana przez dostawcę '{provider.DisplayName}'.");
        }
    }

    /// <summary>The provider's base currency has no rates of its own - it is worth 1 on every day.</summary>
    private static async Task<RateSeries> LoadSeriesAsync(
        ICurrencyRateProvider provider,
        string currencyCode,
        DateRange range,
        CancellationToken cancellationToken)
    {
        if (string.Equals(currencyCode, provider.BaseCurrencyCode, StringComparison.Ordinal))
        {
            return RateSeries.BaseCurrency;
        }

        return RateSeries.From(await provider.GetRatesAsync(currencyCode, range, cancellationToken));
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Brak kursów {SourceCurrency}/{TargetCurrency} u dostawcy {Provider} w okresie {DateFrom:yyyy-MM-dd} - {DateTo:yyyy-MM-dd}")]
    private partial void LogNoData(string provider, string sourceCurrency, string targetCurrency, DateOnly dateFrom, DateOnly dateTo);
}
