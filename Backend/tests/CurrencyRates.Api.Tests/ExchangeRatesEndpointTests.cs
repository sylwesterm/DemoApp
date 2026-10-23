using System.Net;
using System.Net.Http.Json;
using CurrencyRates.Api.Tests.TestSupport;
using CurrencyRates.Application.Features.ExchangeRates;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Api.Tests;

public sealed class ExchangeRatesEndpointTests
{
    [Fact]
    public async Task GetRates_ReturnsCrossRatesStatisticsAndDatesInConfiguredFormat()
    {
        await using var api = new ApiFactory(new Dictionary<string, string?>
        {
            ["Display:DateFormat"] = "dd.MM.yyyy",
        });
        var period = new DateRange(new DateOnly(2026, 10, 14), new DateOnly(2026, 10, 15));
        api.Provider.GetRatesAsync("EUR", period, Arg.Any<CancellationToken>()).Returns(
        [
            new ExchangeRate(period.From, 4.20m),
            new ExchangeRate(period.To, 4.30m),
        ]);
        api.Provider.GetRatesAsync("USD", period, Arg.Any<CancellationToken>()).Returns(
        [
            new ExchangeRate(period.From, 3.50m),
            new ExchangeRate(period.To, 4.00m),
        ]);
        using var client = api.CreateClient();

        var history = await client.GetFromJsonAsync<ExchangeRateHistoryDto>(
            "/api/providers/TEST/rates?sourceCurrency=eur&targetCurrency=USD&dateFrom=2026-10-14&dateTo=2026-10-15");

        history.ShouldNotBeNull();
        history.Provider.ShouldBe(ApiFactory.ProviderKey);
        history.SourceCurrency.ShouldBe("EUR");
        history.TargetCurrency.ShouldBe("USD");
        history.Statistics.ShouldBe(new RateStatisticsDto { Minimum = 1.075m, Maximum = 1.2m, Average = 1.1375m });
        history.Rates.ShouldBe(
        [
            new ExchangeRateDto { Date = period.From, FormattedDate = "14.10.2026", Value = 1.2m },
            new ExchangeRateDto { Date = period.To, FormattedDate = "15.10.2026", Value = 1.075m },
        ]);
    }

    [Fact]
    public async Task GetRates_NoRatesInPeriod_Returns204()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync(
            "/api/providers/TEST/rates?sourceCurrency=EUR&targetCurrency=USD&dateFrom=2026-10-10&dateTo=2026-10-11");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRates_WithoutDates_AsksProviderForTodayInPoland()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/rates?sourceCurrency=EUR&targetCurrency=PLN");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await api.Provider.Received(1).GetRatesAsync(
            "EUR",
            DateRange.SingleDay(ApiFactory.Today),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRates_InvalidParameters_Returns400WithPolishErrorsKeyedByQueryParameter()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync(
            "/api/providers/TEST/rates?sourceCurrency=EU&targetCurrency=&dateFrom=2026-10-17");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.ReadValidationProblemAsync();
        problem.Title.ShouldBe("Nieprawidłowe parametry żądania.");
        problem.Errors.Keys.ShouldBe(["sourceCurrency", "targetCurrency", "dateFrom"], ignoreOrder: true);
        problem.Errors["sourceCurrency"].ShouldBe(["Kod waluty źródłowej musi składać się z trzech liter."]);
        problem.Errors["targetCurrency"].ShouldBe(["Waluta docelowa jest wymagana."]);
        problem.Errors["dateFrom"].ShouldBe(["Data od nie może być późniejsza niż data do."]);
        await api.Provider.DidNotReceiveWithAnyArgs().GetRatesAsync(default!, default!, default);
    }

    [Fact]
    public async Task GetRates_UnparsableDate_Returns400WithPolishMessage()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync(
            "/api/providers/TEST/rates?sourceCurrency=EUR&targetCurrency=USD&dateFrom=abc");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.ReadValidationProblemAsync();
        problem.Title.ShouldBe("Nieprawidłowe parametry żądania.");
        problem.Errors.ShouldContainKey("dateFrom");
        problem.Errors["dateFrom"].ShouldBe(["Wartość 'abc' jest nieprawidłowa."]);
    }

    [Fact]
    public async Task GetRates_CurrencyNotOfferedByProvider_Returns404()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/rates?sourceCurrency=EUR&targetCurrency=GBP");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemAsync();
        problem.Detail.ShouldBe($"Waluta 'GBP' nie jest obsługiwana przez dostawcę '{ApiFactory.ProviderName}'.");
    }
}
