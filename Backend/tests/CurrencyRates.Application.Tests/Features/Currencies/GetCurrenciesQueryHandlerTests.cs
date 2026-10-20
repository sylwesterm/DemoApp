using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Features.Currencies.GetCurrencies;
using CurrencyRates.Application.Tests.TestSupport;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyRates.Application.Tests.Features.Currencies;

/// <summary>
/// Sent through the real container and MediatR pipeline, so the sort order comes from configuration.
/// </summary>
public class GetCurrenciesQueryHandlerTests
{
    private readonly ICurrencyProviderFactory providerFactory = Substitute.For<ICurrencyProviderFactory>();

    public GetCurrenciesQueryHandlerTests()
    {
        var provider = Substitute.For<ICurrencyRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Currency("USD", "dolar amerykański"),
            new Currency("PLN", "złoty polski"),
            new Currency("EUR", "euro"),
        ]);
        providerFactory.GetByKey("NBP").Returns(provider);
    }

    private IMediator CreateMediator(CurrencySortOrder sortOrder) =>
        TestServices.Create(sortOrder: sortOrder, configure: services => services.AddSingleton(providerFactory))
            .GetRequiredService<IMediator>();

    [Theory]
    [InlineData(CurrencySortOrder.CodeAscending, new[] { "EUR", "PLN", "USD" })]
    [InlineData(CurrencySortOrder.CodeDescending, new[] { "USD", "PLN", "EUR" })]
    [InlineData(CurrencySortOrder.NameAscending, new[] { "USD", "EUR", "PLN" })]
    public async Task Send_ReturnsCurrenciesSortedAsConfigured(CurrencySortOrder sortOrder, string[] expectedCodes)
    {
        var result = await CreateMediator(sortOrder).Send(new GetCurrenciesQuery("NBP"));

        result.Select(c => c.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public async Task Send_ReturnsLabelInCodeDashNameFormat()
    {
        var result = await CreateMediator(CurrencySortOrder.CodeAscending).Send(new GetCurrenciesQuery("NBP"));

        result[0].Label.ShouldBe("EUR - euro");
    }

    [Fact]
    public async Task Send_EmptyProviderKey_FailsValidationBeforeHandler()
    {
        var exception = await Should.ThrowAsync<ValidationException>(
            () => CreateMediator(CurrencySortOrder.CodeAscending).Send(new GetCurrenciesQuery("")));

        exception.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe("Należy wybrać dostawcę kursów walut.");
        providerFactory.DidNotReceiveWithAnyArgs().GetByKey(default!);
    }
}
