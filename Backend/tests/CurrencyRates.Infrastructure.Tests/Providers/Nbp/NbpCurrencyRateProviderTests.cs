using AutoMapper;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp;
using CurrencyRates.Infrastructure.Providers.Nbp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Infrastructure.Tests.Providers.Nbp;

public sealed class NbpCurrencyRateProviderTests : IDisposable
{
    private static readonly IMapper Mapper =
        new MapperConfiguration(config => config.AddProfile<NbpMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private readonly INbpApiClient apiClient = Substitute.For<INbpApiClient>();
    private readonly MemoryCache cache = new(new MemoryCacheOptions());
    private readonly NbpOptions options = new() { BaseUrl = new Uri("https://api.nbp.test/api/") };

    public NbpCurrencyRateProviderTests()
    {
        GivenTable("A", ("USD", "dolar amerykański"), ("EUR", "euro"));
        GivenTable("B", ("AFN", "afgani (Afganistan)"), ("USD", "dolar USA (tabela B)"));
    }

    public void Dispose() => cache.Dispose();

    private NbpCurrencyRateProvider CreateProvider() => new(apiClient, cache, Options.Create(options), Mapper);

    private void GivenTable(string table, params (string Code, string Name)[] currencies) =>
        apiClient.GetLatestTableAsync(table, Arg.Any<CancellationToken>())
            .Returns(new NbpTableDto(table, currencies.Select(c => new NbpTableRateDto(c.Name, c.Code)).ToList()));

    private static NbpRateSeriesDto Series(string code, params (string Date, decimal Mid)[] rates) =>
        new(code, rates.Select(r => new NbpRateDto(DateOnly.Parse(r.Date, System.Globalization.CultureInfo.InvariantCulture), r.Mid)).ToList());

    [Fact]
    public void Describes_NbpWithPlnAsBaseCurrency()
    {
        var provider = CreateProvider();

        provider.Key.ShouldBe("NBP");
        provider.DisplayName.ShouldBe("Narodowy Bank Polski");
        provider.BaseCurrencyCode.ShouldBe("PLN");
    }

    [Fact]
    public async Task GetCurrenciesAsync_MergesTablesAddsPlnAndPrefersTableA()
    {
        var currencies = await CreateProvider().GetCurrenciesAsync(CancellationToken.None);

        currencies.Select(c => c.Code).ShouldBe(["PLN", "USD", "EUR", "AFN"], ignoreOrder: true);
        currencies.Single(c => c.Code == "USD").Name.ShouldBe("dolar amerykański");
        currencies.Single(c => c.Code == "PLN").Name.ShouldBe("złoty polski");
    }

    [Fact]
    public async Task GetCurrenciesAsync_SecondCall_UsesCache()
    {
        var provider = CreateProvider();

        await provider.GetCurrenciesAsync(CancellationToken.None);
        await provider.GetCurrenciesAsync(CancellationToken.None);

        await apiClient.Received(1).GetLatestTableAsync("A", Arg.Any<CancellationToken>());
        await apiClient.Received(1).GetLatestTableAsync("B", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrenciesAsync_FailedLoad_IsNotCached()
    {
        apiClient.GetLatestTableAsync("B", Arg.Any<CancellationToken>())
            .Returns(
                _ => throw new ExternalServiceException("Serwis NBP jest niedostępny."),
                _ => new NbpTableDto("B", [new NbpTableRateDto("afgani (Afganistan)", "AFN")]));
        var provider = CreateProvider();

        await Should.ThrowAsync<ExternalServiceException>(() => provider.GetCurrenciesAsync(CancellationToken.None));
        var currencies = await provider.GetCurrenciesAsync(CancellationToken.None);

        currencies.ShouldContain(c => c.Code == "AFN");
    }

    [Fact]
    public async Task GetCurrenciesAsync_InvalidCurrencyFromNbp_ThrowsExternalServiceException()
    {
        GivenTable("A", ("E1R", "euro"));

        var exception = await Should.ThrowAsync<ExternalServiceException>(
            () => CreateProvider().GetCurrenciesAsync(CancellationToken.None));

        exception.Message.ShouldStartWith("Serwis NBP zwrócił nieprawidłowe dane:");
        exception.Message.ShouldContain("'E1R' nie jest prawidłowym trzyliterowym kodem waluty");
    }

    [Fact]
    public async Task GetRatesAsync_UsesTableTheCurrencyIsPublishedIn()
    {
        apiClient.GetRatesAsync("B", "AFN", Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(Series("AFN", ("2026-10-14", 0.060135m)));

        var rates = await CreateProvider().GetRatesAsync("AFN", DateRange.SingleDay(new DateOnly(2026, 10, 14)), CancellationToken.None);

        rates.ShouldHaveSingleItem().ShouldBe(new ExchangeRate(new DateOnly(2026, 10, 14), 0.060135m));
        await apiClient.DidNotReceive().GetRatesAsync("A", "AFN", Arg.Any<DateRange>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRatesAsync_RangeOverLimit_RequestsChunksAndMergesInDateOrder()
    {
        options.MaxDaysPerRequest = 10;
        var from = new DateOnly(2026, 1, 1);
        apiClient.GetRatesAsync("A", "EUR", new DateRange(from, from.AddDays(9)), Arg.Any<CancellationToken>())
            .Returns(Series("EUR", ("2026-01-02", 4.21m)));
        apiClient.GetRatesAsync("A", "EUR", new DateRange(from.AddDays(10), from.AddDays(19)), Arg.Any<CancellationToken>())
            .Returns(Series("EUR", ("2026-01-12", 4.22m)));
        apiClient.GetRatesAsync("A", "EUR", new DateRange(from.AddDays(20), from.AddDays(24)), Arg.Any<CancellationToken>())
            .Returns(Series("EUR", ("2026-01-21", 4.23m)));

        var rates = await CreateProvider().GetRatesAsync("EUR", new DateRange(from, from.AddDays(24)), CancellationToken.None);

        rates.Select(r => r.Value).ShouldBe([4.21m, 4.22m, 4.23m]);
        await apiClient.Received(3).GetRatesAsync("A", "EUR", Arg.Any<DateRange>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRatesAsync_ChunkWithoutData_IsSkipped()
    {
        options.MaxDaysPerRequest = 1;
        var day = new DateOnly(2026, 10, 16);
        apiClient.GetRatesAsync("A", "EUR", DateRange.SingleDay(day), Arg.Any<CancellationToken>())
            .Returns(Series("EUR", ("2026-10-16", 4.3661m)));
        apiClient.GetRatesAsync("A", "EUR", DateRange.SingleDay(day.AddDays(1)), Arg.Any<CancellationToken>())
            .Returns((NbpRateSeriesDto?)null);

        var rates = await CreateProvider().GetRatesAsync("EUR", new DateRange(day, day.AddDays(1)), CancellationToken.None);

        rates.ShouldHaveSingleItem().Date.ShouldBe(day);
    }

    [Fact]
    public async Task GetRatesAsync_NoDataAtAll_ReturnsEmpty()
    {
        apiClient.GetRatesAsync("A", "EUR", Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns((NbpRateSeriesDto?)null);

        (await CreateProvider().GetRatesAsync("EUR", DateRange.SingleDay(new DateOnly(2026, 10, 17)), CancellationToken.None))
            .ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRatesAsync_UnknownCurrency_ReturnsEmptyWithoutCallingNbp()
    {
        (await CreateProvider().GetRatesAsync("XYZ", DateRange.SingleDay(new DateOnly(2026, 10, 16)), CancellationToken.None))
            .ShouldBeEmpty();

        await apiClient.DidNotReceiveWithAnyArgs().GetRatesAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task GetRatesAsync_NonPositiveRateFromNbp_ThrowsExternalServiceException()
    {
        apiClient.GetRatesAsync("A", "EUR", Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(Series("EUR", ("2026-10-16", 0m)));

        (await Should.ThrowAsync<ExternalServiceException>(
                () => CreateProvider().GetRatesAsync("EUR", DateRange.SingleDay(new DateOnly(2026, 10, 16)), CancellationToken.None)))
            .Message.ShouldStartWith("Serwis NBP zwrócił nieprawidłowe dane: Kurs z dnia 2026-10-16 musi być dodatni");
    }
}
