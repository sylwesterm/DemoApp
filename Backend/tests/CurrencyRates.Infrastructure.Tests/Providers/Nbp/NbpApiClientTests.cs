using System.Globalization;
using System.Net;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp;
using CurrencyRates.Infrastructure.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CurrencyRates.Infrastructure.Tests.Providers.Nbp;

/// <summary>
/// The client on its own, without the resilience pipeline: checks URLs and how each outcome is translated.
/// </summary>
public class NbpApiClientTests
{
    private static readonly DateRange October15To16 = new(new DateOnly(2026, 10, 15), new DateOnly(2026, 10, 16));

    private static (NbpApiClient Client, StubHttpMessageHandler Handler) CreateClient(
        Func<HttpRequestMessage, int, HttpResponseMessage> respond)
    {
        var handler = new StubHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.nbp.test/api/") };
        return (new NbpApiClient(httpClient, NullLogger<NbpApiClient>.Instance), handler);
    }

    [Fact]
    public async Task GetLatestTableAsync_ParsesNbpResponse()
    {
        var (client, handler) = CreateClient((_, _) => StubHttpMessageHandler.Json(NbpJson.TableA));

        var table = await client.GetLatestTableAsync("A", CancellationToken.None);

        table.Table.ShouldBe("A");
        table.Rates.Select(r => (r.Code, r.Currency)).ShouldBe([("USD", "dolar amerykański"), ("EUR", "euro")]);
        handler.RequestedUris.ShouldHaveSingleItem().AbsoluteUri
            .ShouldBe("https://api.nbp.test/api/exchangerates/tables/A/?format=json");
    }

    [Fact]
    public async Task GetRatesAsync_ParsesNbpResponseAndBuildsUrl()
    {
        var (client, handler) = CreateClient((_, _) => StubHttpMessageHandler.Json(NbpJson.EurRates));

        var series = await client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None);

        series.ShouldNotBeNull().Rates.Select(r => (r.EffectiveDate, r.Mid))
            .ShouldBe([(new DateOnly(2026, 10, 15), 4.3691m), (new DateOnly(2026, 10, 16), 4.3661m)]);
        handler.RequestedUris.ShouldHaveSingleItem().AbsoluteUri
            .ShouldBe("https://api.nbp.test/api/exchangerates/rates/A/EUR/2026-10-15/2026-10-16/?format=json");
    }

    [Fact]
    public async Task GetRatesAsync_CultureWithBuddhistCalendar_StillUsesGregorianYearInUrl()
    {
        var (client, handler) = CreateClient((_, _) => StubHttpMessageHandler.Json(NbpJson.EurRates));
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH"); // year 2026 = 2569 in this culture

        try
        {
            await client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        handler.RequestedUris.ShouldHaveSingleItem().AbsoluteUri.ShouldContain("/2026-10-15/2026-10-16/");
    }

    [Fact]
    public async Task GetRatesAsync_NotFound_ReturnsNull()
    {
        var (client, _) = CreateClient((_, _) => StubHttpMessageHandler.Json("\"404 NotFound - Not Found - Brak danych\"", HttpStatusCode.NotFound));

        (await client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None)).ShouldBeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetRatesAsync_ErrorStatus_ThrowsExternalServiceException(HttpStatusCode status)
    {
        var (client, _) = CreateClient((_, _) => StubHttpMessageHandler.Status(status));

        var exception = await Should.ThrowAsync<ExternalServiceException>(
            () => client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None));

        exception.ShouldNotBeOfType<ExternalServiceTimeoutException>();
        exception.Message.ShouldStartWith($"Serwis NBP zwrócił błąd {(int)status}");
    }

    [Theory]
    [InlineData("not json")]
    [InlineData(NbpJson.RatesWithoutRatesField)]
    [InlineData("""{"code":"EUR","rates":[{"effectiveDate":"2026-10-15","mid":null}]}""")]
    public async Task GetRatesAsync_UnexpectedJson_ThrowsExternalServiceException(string json)
    {
        var (client, _) = CreateClient((_, _) => StubHttpMessageHandler.Json(json));

        (await Should.ThrowAsync<ExternalServiceException>(
                () => client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None)))
            .Message.ShouldBe("Serwis NBP zwrócił dane w nieoczekiwanym formacie.");
    }

    [Fact]
    public async Task GetLatestTableAsync_EmptyArray_ThrowsExternalServiceException()
    {
        var (client, _) = CreateClient((_, _) => StubHttpMessageHandler.Json("[]"));

        (await Should.ThrowAsync<ExternalServiceException>(() => client.GetLatestTableAsync("B", CancellationToken.None)))
            .Message.ShouldBe("Serwis NBP nie zwrócił tabeli kursów B.");
    }

    [Fact]
    public async Task GetRatesAsync_NetworkError_ThrowsExternalServiceException()
    {
        var (client, _) = CreateClient((_, _) => throw new HttpRequestException("connection refused"));

        (await Should.ThrowAsync<ExternalServiceException>(
                () => client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None)))
            .Message.ShouldBe("Nie udało się połączyć z serwisem NBP.");
    }

    [Fact]
    public async Task GetRatesAsync_ResilienceTimeout_ThrowsExternalServiceTimeoutException()
    {
        var (client, _) = CreateClient((_, _) => throw new TimeoutRejectedException());

        (await Should.ThrowAsync<ExternalServiceTimeoutException>(
                () => client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None)))
            .Message.ShouldBe("Serwis NBP nie odpowiedział w wymaganym czasie.");
    }

    [Fact]
    public async Task GetRatesAsync_OpenCircuit_ThrowsExternalServiceException()
    {
        var (client, _) = CreateClient((_, _) => throw new BrokenCircuitException());

        (await Should.ThrowAsync<ExternalServiceException>(
                () => client.GetRatesAsync("A", "EUR", October15To16, CancellationToken.None)))
            .Message.ShouldBe("Serwis NBP jest chwilowo niedostępny. Spróbuj ponownie za chwilę.");
    }

    [Fact]
    public async Task GetRatesAsync_CancelledByCaller_PropagatesCancellation()
    {
        var (client, _) = CreateClient((_, _) => throw new TaskCanceledException());
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(
            () => client.GetRatesAsync("A", "EUR", October15To16, cancellation.Token));
    }
}
