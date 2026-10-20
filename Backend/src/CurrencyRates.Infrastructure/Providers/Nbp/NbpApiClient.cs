using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp.Models;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Typed HTTP client of the NBP API. Retries, timeouts and the circuit breaker are added
/// by the resilience handler configured in DI, so this class only translates outcomes.
/// </summary>
public sealed partial class NbpApiClient(HttpClient httpClient, ILogger<NbpApiClient> logger) : INbpApiClient
{
    // Missing or null required fields make deserialization fail instead of producing half-empty objects.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public async Task<NbpTableDto> GetLatestTableAsync(string table, CancellationToken cancellationToken)
    {
        var tables = await GetAsync<List<NbpTableDto>>($"exchangerates/tables/{table}/?format=json", cancellationToken);

        return tables?.FirstOrDefault()
               ?? throw new ExternalServiceException($"Serwis NBP nie zwrócił tabeli kursów {table}.");
    }

    public Task<NbpRateSeriesDto?> GetRatesAsync(
        string table,
        string currencyCode,
        DateRange range,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(range);

        // Invariant culture: a culture with a non-Gregorian calendar would otherwise produce a wrong year.
        var uri = string.Create(
            CultureInfo.InvariantCulture,
            $"exchangerates/rates/{table}/{currencyCode}/{range.From:yyyy-MM-dd}/{range.To:yyyy-MM-dd}/?format=json");

        return GetAsync<NbpRateSeriesDto>(uri, cancellationToken);
    }

    /// <returns>The deserialized body, or <c>null</c> for HTTP 404 ("Brak danych").</returns>
    private async Task<T?> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            using var response = await httpClient.GetAsync(new Uri(relativeUri, UriKind.Relative), cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                LogUnexpectedStatus((int)response.StatusCode, relativeUri);
                throw new ExternalServiceException(
                    $"Serwis NBP zwrócił błąd {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                   ?? throw new ExternalServiceException("Serwis NBP zwrócił pustą odpowiedź.");
        }
        catch (TimeoutRejectedException exception)
        {
            throw new ExternalServiceTimeoutException("Serwis NBP nie odpowiedział w wymaganym czasie.", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw new ExternalServiceException(
                "Serwis NBP jest chwilowo niedostępny. Spróbuj ponownie za chwilę.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalServiceException("Nie udało się połączyć z serwisem NBP.", exception);
        }
        catch (JsonException exception)
        {
            throw new ExternalServiceException("Serwis NBP zwrócił dane w nieoczekiwanym formacie.", exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Cancelled by HttpClient.Timeout rather than by the caller.
            throw new ExternalServiceTimeoutException("Serwis NBP nie odpowiedział w wymaganym czasie.", exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Serwis NBP zwrócił nieoczekiwany status {StatusCode} dla {RequestUri}")]
    private partial void LogUnexpectedStatus(int statusCode, string requestUri);
}
