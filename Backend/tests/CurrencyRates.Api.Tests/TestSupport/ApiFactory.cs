using CurrencyRates.Application.Abstractions;
using CurrencyRates.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Serilog.Core;

namespace CurrencyRates.Api.Tests.TestSupport;

/// <summary>
/// The real API - Program.cs, DI, middleware, error handling and Serilog - with three replacements:
/// a fake provider instead of NBP (no network), a fixed "today" and a sink capturing log events.
/// Create one per test, so each test configures its own fake.
/// </summary>
internal sealed class ApiFactory(IReadOnlyDictionary<string, string?>? settings = null) : WebApplicationFactory<Program>
{
    public const string ProviderKey = "TEST";
    public const string ProviderName = "Dostawca testowy";

    /// <summary>Friday, 2026-10-16 - noon in Poland.</summary>
    public static readonly DateOnly Today = new(2026, 10, 16);

    public ICurrencyRateProvider Provider { get; } = CreateProvider();

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 10, 16, 10, 0, 0, TimeSpan.Zero));

    public CapturingSink Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (settings is not null)
        {
            builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(settings));
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICurrencyRateProvider>();
            services.AddScoped(_ => Provider);

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);

            // Program.cs reads sinks from DI (ReadFrom.Services), so this one receives every log event.
            services.AddSingleton<ILogEventSink>(Logs);
        });
    }

    private static ICurrencyRateProvider CreateProvider()
    {
        var provider = Substitute.For<ICurrencyRateProvider>();
        provider.Key.Returns(ProviderKey);
        provider.DisplayName.Returns(ProviderName);
        provider.BaseCurrencyCode.Returns("PLN");
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Currency("PLN", "złoty polski"),
            new Currency("USD", "dolar amerykański"),
            new Currency("EUR", "euro"),
        ]);
        provider.GetRatesAsync(Arg.Any<string>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ExchangeRate>());
        return provider;
    }
}
