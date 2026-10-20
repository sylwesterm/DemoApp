using System.Net;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp;
using CurrencyRates.Infrastructure.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyRates.Infrastructure.Tests.Http;

/// <summary>
/// The NBP client as registered in DI - with the real resilience pipeline - and only the network stubbed.
/// </summary>
public class ResiliencePipelineTests
{
    private static readonly DateRange AnyRange = DateRange.SingleDay(new DateOnly(2026, 10, 15));

    private static INbpApiClient CreateClient(StubHttpMessageHandler handler, int maxRetryAttempts = 3)
    {
        var services = TestConfiguration.CreateServices(
            new() { ["CurrencyProviders:Nbp:Resilience:MaxRetryAttempts"] = maxRetryAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            configure: s => s.AddHttpClient<INbpApiClient, NbpApiClient>().ConfigurePrimaryHttpMessageHandler(() => handler));

        return services.GetRequiredService<INbpApiClient>();
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task TransientError_IsRetriedUntilSuccess(HttpStatusCode transientStatus)
    {
        var handler = new StubHttpMessageHandler((_, call) =>
            call < 3 ? StubHttpMessageHandler.Status(transientStatus) : StubHttpMessageHandler.Json(NbpJson.EurRates));

        var series = await CreateClient(handler).GetRatesAsync("A", "EUR", AnyRange, CancellationToken.None);

        series.ShouldNotBeNull();
        handler.CallCount.ShouldBe(3);
    }

    [Fact]
    public async Task NetworkError_IsRetried()
    {
        var handler = new StubHttpMessageHandler((_, call) =>
            call == 1 ? throw new HttpRequestException("connection reset") : StubHttpMessageHandler.Json(NbpJson.EurRates));

        await CreateClient(handler).GetRatesAsync("A", "EUR", AnyRange, CancellationToken.None);

        handler.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task PersistentError_GivesUpAfterConfiguredRetries()
    {
        var handler = new StubHttpMessageHandler((_, _) => StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable));

        await Should.ThrowAsync<ExternalServiceException>(
            () => CreateClient(handler, maxRetryAttempts: 2).GetRatesAsync("A", "EUR", AnyRange, CancellationToken.None));

        handler.CallCount.ShouldBe(3); // the first attempt + 2 retries
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task NonTransientStatus_IsNotRetried(HttpStatusCode status)
    {
        var handler = new StubHttpMessageHandler((_, _) => StubHttpMessageHandler.Status(status));
        var client = CreateClient(handler);

        try
        {
            await client.GetRatesAsync("A", "EUR", AnyRange, CancellationToken.None);
        }
        catch (ExternalServiceException)
        {
            // 400 is reported as an error, 404 as "no data" - either way it must not be retried.
        }

        handler.CallCount.ShouldBe(1);
    }
}
