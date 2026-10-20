using CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;
using CurrencyRates.Application.Tests.TestSupport;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace CurrencyRates.Application.Tests.Features.ExchangeRates;

public class GetExchangeRatesQueryValidatorTests
{
    // 12:00 in Poland on 2026-10-19.
    private static readonly DateOnly Today = Rates.Day("2026-10-19");
    private readonly GetExchangeRatesQueryValidator validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.Zero)));

    private static GetExchangeRatesQuery Query(
        string provider = "NBP",
        string source = "EUR",
        string target = "USD",
        DateOnly? from = null,
        DateOnly? to = null) => new(provider, source, target, from, to);

    [Fact]
    public void ValidQuery_HasNoErrors()
    {
        validator.TestValidate(Query(from: Today.AddDays(-30), to: Today)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DatesNotProvided_DefaultToTodayAndAreValid()
    {
        validator.TestValidate(Query(from: null, to: null)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyProvider_ReportsError()
    {
        validator.TestValidate(Query(provider: ""))
            .ShouldHaveValidationErrorFor(q => q.ProviderKey)
            .WithErrorMessage("Należy wybrać dostawcę kursów walut.");
    }

    [Fact]
    public void EmptySourceCurrency_ReportsOnlyRequiredError()
    {
        var result = validator.TestValidate(Query(source: ""));

        result.ShouldHaveValidationErrorFor(q => q.SourceCurrency)
            .WithErrorMessage("Waluta źródłowa jest wymagana.");
        result.Errors.Count(e => e.PropertyName == nameof(GetExchangeRatesQuery.SourceCurrency)).ShouldBe(1);
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void MalformedTargetCurrency_ReportsError(string target)
    {
        validator.TestValidate(Query(target: target))
            .ShouldHaveValidationErrorFor(q => q.TargetCurrency)
            .WithErrorMessage("Kod waluty docelowej musi składać się z trzech liter.");
    }

    [Fact]
    public void SameCurrencies_IgnoringCase_ReportsError()
    {
        validator.TestValidate(Query(source: "EUR", target: "eur"))
            .ShouldHaveValidationErrorFor(q => q.TargetCurrency)
            .WithErrorMessage("Waluta docelowa musi być inna niż waluta źródłowa.");
    }

    [Fact]
    public void DateToInFuture_ReportsOnlyFutureError()
    {
        var result = validator.TestValidate(Query(from: Today, to: Today.AddDays(400)));

        result.ShouldHaveValidationErrorFor(q => q.DateTo)
            .WithErrorMessage("Data do nie może być datą z przyszłości.");
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
    }

    [Fact]
    public void DateFromAfterDateTo_ReportsError()
    {
        validator.TestValidate(Query(from: Today, to: Today.AddDays(-1)))
            .ShouldHaveValidationErrorFor(q => q.DateFrom)
            .WithErrorMessage("Data od nie może być późniejsza niż data do.");
    }

    [Fact]
    public void OnlyDateFromInFuture_ComparedWithDefaultDateTo_ReportsError()
    {
        // DateTo defaults to today, so a DateFrom of tomorrow is after it.
        validator.TestValidate(Query(from: Today.AddDays(1), to: null))
            .ShouldHaveValidationErrorFor(q => q.DateFrom);
    }

    [Theory]
    [InlineData(366, false)]
    [InlineData(367, true)]
    public void RangeLength_LimitedTo366Days(int days, bool expectError)
    {
        var result = validator.TestValidate(Query(from: Today.AddDays(-(days - 1)), to: Today));

        if (expectError)
        {
            result.ShouldHaveValidationErrorFor(q => q.DateFrom)
                .WithErrorMessage("Zakres dat nie może przekraczać 366 dni.");
        }
        else
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
