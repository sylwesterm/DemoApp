using System.Net;
using System.Net.Http.Json;
using CurrencyRates.Api.Tests.TestSupport;
using CurrencyRates.Application.Features.Currencies;

namespace CurrencyRates.Api.Tests;

public sealed class CurrenciesEndpointTests
{
    [Fact]
    public async Task GetCurrencies_ReturnsCurrenciesWithLabelsSortedByCode()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var currencies = await client.GetFromJsonAsync<List<CurrencyDto>>("/api/providers/test/currencies");

        currencies.ShouldNotBeNull();
        currencies.Select(currency => currency.Code).ShouldBe(["EUR", "PLN", "USD"]);
        currencies[0].ShouldBe(new CurrencyDto { Code = "EUR", Name = "euro", Label = "EUR - euro" });
    }

    [Fact]
    public async Task GetCurrencies_SortOrderComesFromConfiguration()
    {
        await using var api = new ApiFactory(new Dictionary<string, string?>
        {
            ["Display:CurrencySortOrder"] = "NameDescending",
        });
        using var client = api.CreateClient();

        var currencies = await client.GetFromJsonAsync<List<CurrencyDto>>("/api/providers/TEST/currencies");

        currencies.ShouldNotBeNull()
            .Select(currency => currency.Name)
            .ShouldBe(["złoty polski", "euro", "dolar amerykański"]);
    }

    [Fact]
    public async Task GetCurrencies_UnknownProvider_Returns404WithPolishProblemDetails()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/ECB/currencies");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Nie znaleziono zasobu.");
        problem.Detail.ShouldBe("Nie znaleziono dostawcy kursów walut 'ECB'.");
    }
}
