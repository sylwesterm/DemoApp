using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Features.Providers.GetProviders;
using CurrencyRates.Application.Tests.TestSupport;

namespace CurrencyRates.Application.Tests.Features.Providers;

public class GetProvidersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEveryRegisteredProvider()
    {
        var nbp = Provider("NBP", "Narodowy Bank Polski", "PLN");
        var ecb = Provider("ECB", "Europejski Bank Centralny", "EUR");
        var factory = Substitute.For<ICurrencyProviderFactory>();
        factory.GetAll().Returns([nbp, ecb]);

        var result = await new GetProvidersQueryHandler(factory, TestServices.CreateMapper())
            .Handle(new GetProvidersQuery(), CancellationToken.None);

        result.Select(p => (p.Key, p.DisplayName, p.BaseCurrencyCode)).ShouldBe(
        [
            ("NBP", "Narodowy Bank Polski", "PLN"),
            ("ECB", "Europejski Bank Centralny", "EUR"),
        ]);
    }

    private static ICurrencyRateProvider Provider(string key, string displayName, string baseCurrency)
    {
        var provider = Substitute.For<ICurrencyRateProvider>();
        provider.Key.Returns(key);
        provider.DisplayName.Returns(displayName);
        provider.BaseCurrencyCode.Returns(baseCurrency);
        return provider;
    }
}
