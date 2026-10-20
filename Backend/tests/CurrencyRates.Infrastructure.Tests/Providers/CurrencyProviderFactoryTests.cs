using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Infrastructure.Providers;

namespace CurrencyRates.Infrastructure.Tests.Providers;

public class CurrencyProviderFactoryTests
{
    private static ICurrencyRateProvider Provider(string key)
    {
        var provider = Substitute.For<ICurrencyRateProvider>();
        provider.Key.Returns(key);
        return provider;
    }

    [Fact]
    public void GetAll_ReturnsProvidersInRegistrationOrder()
    {
        var nbp = Provider("NBP");
        var ecb = Provider("ECB");

        new CurrencyProviderFactory([nbp, ecb]).GetAll().ShouldBe([nbp, ecb]);
    }

    [Theory]
    [InlineData("NBP")]
    [InlineData("nbp")]
    [InlineData("Nbp")]
    public void GetByKey_IgnoresCase(string key)
    {
        var nbp = Provider("NBP");

        new CurrencyProviderFactory([nbp]).GetByKey(key).ShouldBeSameAs(nbp);
    }

    [Fact]
    public void GetByKey_UnknownKey_ThrowsNotFoundWithPolishMessage()
    {
        Should.Throw<NotFoundException>(() => new CurrencyProviderFactory([Provider("NBP")]).GetByKey("ECB"))
            .Message.ShouldBe("Nie znaleziono dostawcy kursów walut 'ECB'.");
    }

    [Fact]
    public void Constructor_DuplicateKeys_Throws()
    {
        Should.Throw<InvalidOperationException>(() => new CurrencyProviderFactory([Provider("NBP"), Provider("nbp")]))
            .Message.ShouldBe("Zarejestrowano więcej niż jednego dostawcę o kluczu 'NBP'.");
    }
}
