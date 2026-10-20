using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;
using CurrencyRates.Application.Services;
using CurrencyRates.Application.Tests.TestSupport;
using CurrencyRates.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace CurrencyRates.Application.Tests.Features.ExchangeRates;

public class GetExchangeRatesQueryHandlerTests
{
    private readonly ICurrencyRateProvider provider = Substitute.For<ICurrencyRateProvider>();
    private readonly ICurrencyProviderFactory providerFactory = Substitute.For<ICurrencyProviderFactory>();

    // 2026-10-18 22:30 UTC = 2026-10-19 00:30 in Poland.
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 10, 18, 22, 30, 0, TimeSpan.Zero));

    public GetExchangeRatesQueryHandlerTests()
    {
        provider.Key.Returns("NBP");
        provider.DisplayName.Returns("Narodowy Bank Polski");
        provider.BaseCurrencyCode.Returns("PLN");
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Currency("EUR", "euro"),
            new Currency("USD", "dolar amerykański"),
            new Currency("PLN", "złoty polski"),
        ]);
        providerFactory.GetByKey("NBP").Returns(provider);
    }

    private GetExchangeRatesQueryHandler CreateHandler(string dateFormat = "yyyy-MM-dd") => new(
        providerFactory,
        new CrossRateCalculator(),
        new RateStatisticsCalculator(),
        timeProvider,
        TestServices.CreateMapper(dateFormat),
        NullLogger<GetExchangeRatesQueryHandler>.Instance);

    private void GivenRates(string code, IReadOnlyList<ExchangeRate> rates) =>
        provider.GetRatesAsync(code, Arg.Any<DateRange>(), Arg.Any<CancellationToken>()).Returns(rates);

    [Fact]
    public async Task Handle_TwoForeignCurrencies_ReturnsCrossRatesWithStatistics()
    {
        GivenRates("EUR", Rates.Of(("2026-10-15", 4.25m), ("2026-10-16", 4.26m)));
        GivenRates("USD", Rates.Of(("2026-10-15", 3.90m), ("2026-10-16", 3.92m)));
        var query = new GetExchangeRatesQuery("NBP", "EUR", "USD", Rates.Day("2026-10-15"), Rates.Day("2026-10-16"));

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Provider.ShouldBe("NBP");
        result.SourceCurrency.ShouldBe("EUR");
        result.TargetCurrency.ShouldBe("USD");
        result.Rates.Select(r => r.Value).ShouldBe([1.08974359m, 1.08673469m]);
        result.Statistics.Minimum.ShouldBe(1.08673469m);
        result.Statistics.Maximum.ShouldBe(1.08974359m);
        result.Statistics.Average.ShouldBe(1.08823914m);
    }

    [Fact]
    public async Task Handle_TargetIsBaseCurrency_DoesNotRequestRatesForIt()
    {
        GivenRates("EUR", Rates.Of(("2026-10-15", 4.25m)));
        var query = new GetExchangeRatesQuery("NBP", "EUR", "PLN", Rates.Day("2026-10-15"), Rates.Day("2026-10-15"));

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull().Rates.ShouldHaveSingleItem().Value.ShouldBe(4.25m);
        await provider.DidNotReceive().GetRatesAsync("PLN", Arg.Any<DateRange>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoRatesInPeriod_ReturnsNull()
    {
        GivenRates("EUR", []);
        GivenRates("USD", []);
        var query = new GetExchangeRatesQuery("NBP", "EUR", "USD", Rates.Day("2026-10-17"), Rates.Day("2026-10-18"));

        (await CreateHandler().Handle(query, CancellationToken.None)).ShouldBeNull();
    }

    [Fact]
    public async Task Handle_DatesNotProvided_RequestsTodayInPoland()
    {
        GivenRates("EUR", []);
        var query = new GetExchangeRatesQuery("NBP", "EUR", "PLN", DateFrom: null, DateTo: null);

        await CreateHandler().Handle(query, CancellationToken.None);

        await provider.Received(1).GetRatesAsync(
            "EUR",
            DateRange.SingleDay(Rates.Day("2026-10-19")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LowerCaseCodes_AreNormalized()
    {
        GivenRates("EUR", Rates.Of(("2026-10-15", 4.25m)));
        var query = new GetExchangeRatesQuery("NBP", "eur", "pln", Rates.Day("2026-10-15"), Rates.Day("2026-10-15"));

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull().SourceCurrency.ShouldBe("EUR");
        result.TargetCurrency.ShouldBe("PLN");
    }

    [Fact]
    public async Task Handle_UnsupportedCurrency_ThrowsNotFoundWithPolishMessage()
    {
        var query = new GetExchangeRatesQuery("NBP", "EUR", "XYZ", Rates.Day("2026-10-15"), Rates.Day("2026-10-15"));

        var exception = await Should.ThrowAsync<NotFoundException>(() => CreateHandler().Handle(query, CancellationToken.None));

        exception.Message.ShouldBe("Waluta 'XYZ' nie jest obsługiwana przez dostawcę 'Narodowy Bank Polski'.");
        await provider.DidNotReceive().GetRatesAsync(Arg.Any<string>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConfiguredDateFormat_IsAppliedToRates()
    {
        GivenRates("EUR", Rates.Of(("2026-10-15", 4.25m)));
        var query = new GetExchangeRatesQuery("NBP", "EUR", "PLN", Rates.Day("2026-10-15"), Rates.Day("2026-10-15"));

        var result = await CreateHandler(dateFormat: "d MMMM yyyy").Handle(query, CancellationToken.None);

        var rate = result.ShouldNotBeNull().Rates.ShouldHaveSingleItem();
        rate.Date.ShouldBe(Rates.Day("2026-10-15"));
        rate.FormattedDate.ShouldBe("15 października 2026");
    }
}
