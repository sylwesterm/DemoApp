using System.Net;
using System.Net.Http.Headers;
using CurrencyRates.Api.Tests.TestSupport;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;
using NSubstitute.ExceptionExtensions;
using Serilog.Events;

namespace CurrencyRates.Api.Tests;

/// <summary>How exceptions thrown below the controllers become HTTP responses and log entries.</summary>
public sealed class ErrorHandlingTests
{
    [Fact]
    public async Task ProviderError_Returns502AndLogsErrorWithStackTrace()
    {
        await using var api = new ApiFactory();
        var exception = new ExternalServiceException("Serwis testowy zwrócił błąd.", new HttpRequestException("Connection refused"));
        api.Provider.GetCurrenciesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/currencies");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Błąd dostawcy kursów walut.");
        problem.Detail.ShouldBe("Serwis testowy zwrócił błąd.");

        var logged = api.Logs.From(CapturingSink.ExceptionHandler).ShouldHaveSingleItem();
        logged.Level.ShouldBe(LogEventLevel.Error);
        logged.Exception.ShouldBeSameAs(exception);
        api.Logs.All.Count(logEvent => logEvent.Level >= LogEventLevel.Error).ShouldBe(1);
    }

    [Fact]
    public async Task ProviderTimeout_Returns504()
    {
        await using var api = new ApiFactory();
        api.Provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ExternalServiceTimeoutException("Serwis testowy nie odpowiedział w wymaganym czasie."));
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/currencies");

        response.StatusCode.ShouldBe(HttpStatusCode.GatewayTimeout);
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Dostawca kursów walut nie odpowiedział w wymaganym czasie.");
        problem.Detail.ShouldBe("Serwis testowy nie odpowiedział w wymaganym czasie.");
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutInternalDetailsAndLogsStackTrace()
    {
        await using var api = new ApiFactory();
        var exception = new InvalidOperationException("Wewnętrzny szczegół, którego klient nie powinien zobaczyć");
        api.Provider.GetCurrenciesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/currencies");

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        (await response.Content.ReadAsStringAsync()).ShouldNotContain("Wewnętrzny szczegół");
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Wystąpił nieoczekiwany błąd.");
        problem.Detail.ShouldBe("Spróbuj ponownie później. Szczegóły błędu zapisano w dzienniku aplikacji.");

        var logged = api.Logs.From(CapturingSink.ExceptionHandler).ShouldHaveSingleItem();
        logged.Level.ShouldBe(LogEventLevel.Error);
        logged.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task DomainRuleViolation_Returns400AndLogsWarningWithStackTrace()
    {
        await using var api = new ApiFactory();
        var exception = new DomainException("Kurs waluty musi być większy od zera.");
        api.Provider.GetRatesAsync(Arg.Any<string>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/TEST/rates?sourceCurrency=EUR&targetCurrency=USD");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Nieprawidłowe dane.");
        problem.Detail.ShouldBe("Kurs waluty musi być większy od zera.");

        var logged = api.Logs.From(CapturingSink.ExceptionHandler).ShouldHaveSingleItem();
        logged.Level.ShouldBe(LogEventLevel.Warning);
        logged.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task RejectedRequest_LogsOneLineWarningWithoutStackTrace()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        using var response = await client.GetAsync("/api/providers/ECB/currencies");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var logged = api.Logs.From(CapturingSink.ExceptionHandler).ShouldHaveSingleItem();
        logged.Level.ShouldBe(LogEventLevel.Warning);
        logged.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task ClientNotAcceptingJson_StillGetsPolishProblemDetails()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/providers/ECB/currencies");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        using var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemAsync();
        problem.Title.ShouldBe("Nie znaleziono zasobu.");
        // Without the fallback the exception middleware would log the 404 again, in English, as an error.
        api.Logs.All.ShouldNotContain(logEvent => logEvent.Level >= LogEventLevel.Error);
    }
}
