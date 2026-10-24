using System.Net;
using CurrencyRates.Api.RateLimiting;
using CurrencyRates.Api.Tests.TestSupport;
using FluentValidation.TestHelper;
using Serilog.Events;

namespace CurrencyRates.Api.Tests;

public sealed class RateLimitingTests
{
    private static ApiFactory CreateApi(int permitLimit) =>
        new(new Dictionary<string, string?>
        {
            ["RateLimiting:PermitLimit"] = permitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["RateLimiting:Window"] = "00:01:00",
        });

    [Fact]
    public async Task RequestsOverTheLimit_Get429WithPolishProblemDetailsAndRetryAfter()
    {
        await using var api = CreateApi(permitLimit: 2);
        using var client = api.CreateClient();

        (await client.GetAsync("/api/providers")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync("/api/providers/TEST/currencies")).StatusCode.ShouldBe(HttpStatusCode.OK);
        using var rejected = await client.GetAsync("/api/providers");

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        var problem = await rejected.ReadProblemAsync();
        problem.Title.ShouldBe("Zbyt wiele zapytań.");
        problem.Detail.ShouldNotBeNull().ShouldStartWith("Dozwolonych jest 2 zapytań w ciągu 60 s. Spróbuj ponownie za ");
        var retryAfter = int.Parse(rejected.Headers.GetValues("Retry-After").Single(), System.Globalization.CultureInfo.InvariantCulture);
        retryAfter.ShouldBeInRange(1, 60);

        var logged = api.Logs.From("CurrencyRates.Api.RateLimiting.RateLimitingExtensions").ShouldHaveSingleItem();
        logged.Level.ShouldBe(LogEventLevel.Warning);
        logged.RenderMessage(System.Globalization.CultureInfo.InvariantCulture).ShouldContain("Przekroczono limit zapytań");
    }

    [Fact]
    public async Task ApiDocumentation_IsNotLimited()
    {
        await using var api = CreateApi(permitLimit: 1);
        using var client = api.CreateClient();
        await client.GetAsync("/api/providers");
        (await client.GetAsync("/api/providers")).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        (await client.GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(0, "00:01:00", nameof(RateLimitingOptions.PermitLimit))]
    [InlineData(60, "00:00:00", nameof(RateLimitingOptions.Window))]
    [InlineData(60, "02:00:00", nameof(RateLimitingOptions.Window))]
    public void InvalidSettings_AreRejectedWithPolishMessage(int permitLimit, string window, string property)
    {
        var options = new RateLimitingOptions { PermitLimit = permitLimit, Window = TimeSpan.Parse(window, System.Globalization.CultureInfo.InvariantCulture) };

        var result = new RateLimitingOptionsValidator().TestValidate(options);

        result.ShouldHaveValidationErrorFor(property).WithErrorMessage(
            property == nameof(RateLimitingOptions.PermitLimit)
                ? "Limit zapytań (RateLimiting:PermitLimit) musi wynosić od 1 do 10000."
                : "Okno limitu zapytań (RateLimiting:Window) musi trwać od 1 sekundy do 1 godziny.");
    }
}
