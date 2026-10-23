using System.Net.Http.Json;
using CurrencyRates.Api.Tests.TestSupport;
using CurrencyRates.Application.Features.Providers;

namespace CurrencyRates.Api.Tests;

public sealed class ProvidersEndpointTests
{
    [Fact]
    public async Task GetProviders_ReturnsRegisteredProviders()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var providers = await client.GetFromJsonAsync<List<ProviderDto>>("/api/providers");

        providers.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe(new ProviderDto
        {
            Key = ApiFactory.ProviderKey,
            DisplayName = ApiFactory.ProviderName,
            BaseCurrencyCode = "PLN",
        });
    }
}
