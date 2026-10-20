using CurrencyRates.Infrastructure.Http;
using CurrencyRates.Infrastructure.Providers.Nbp;
using FluentValidation.TestHelper;

namespace CurrencyRates.Infrastructure.Tests.Providers.Nbp;

public class NbpOptionsValidatorTests
{
    private readonly NbpOptionsValidator validator = new();

    private static NbpOptions ValidOptions() => new() { BaseUrl = new Uri("https://api.nbp.pl/api/") };

    [Fact]
    public void DefaultsWithBaseUrl_AreValid()
    {
        validator.TestValidate(ValidOptions()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MissingBaseUrl_ReportsPolishError()
    {
        var options = ValidOptions();
        options.BaseUrl = null;

        validator.TestValidate(options).ShouldHaveValidationErrorFor(o => o.BaseUrl)
            .WithErrorMessage("Adres API NBP (CurrencyProviders:Nbp:BaseUrl) musi być bezwzględnym adresem http(s).");
    }

    [Fact]
    public void BaseUrlWithoutTrailingSlash_ReportsPolishError()
    {
        // Without the slash, HttpClient would drop the "/api" segment when combining relative URIs.
        var options = ValidOptions();
        options.BaseUrl = new Uri("https://api.nbp.pl/api");

        validator.TestValidate(options).ShouldHaveValidationErrorFor(o => o.BaseUrl)
            .WithErrorMessage("Adres API NBP (CurrencyProviders:Nbp:BaseUrl) musi kończyć się znakiem '/'.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(368)]
    public void MaxDaysPerRequestOutsideNbpLimit_ReportsError(int days)
    {
        var options = ValidOptions();
        options.MaxDaysPerRequest = days;

        validator.TestValidate(options).ShouldHaveValidationErrorFor(o => o.MaxDaysPerRequest);
    }

    [Fact]
    public void TotalTimeoutShorterThanAttemptTimeout_ReportsNestedError()
    {
        var options = ValidOptions();
        options.Resilience = new HttpResilienceSettings
        {
            AttemptTimeout = TimeSpan.FromSeconds(10),
            TotalRequestTimeout = TimeSpan.FromSeconds(5),
        };

        validator.TestValidate(options).ShouldHaveValidationErrorFor("Resilience.TotalRequestTimeout")
            .WithErrorMessage("Całkowity limit czasu (TotalRequestTimeout) nie może być krótszy niż limit czasu próby (AttemptTimeout).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void RetryAttemptsOutOfRange_ReportsNestedError(int attempts)
    {
        var options = ValidOptions();
        options.Resilience.MaxRetryAttempts = attempts;

        validator.TestValidate(options).ShouldHaveValidationErrorFor("Resilience.MaxRetryAttempts");
    }
}
