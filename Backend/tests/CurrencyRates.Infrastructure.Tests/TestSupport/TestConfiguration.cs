using CurrencyRates.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Infrastructure.Tests.TestSupport;

/// <summary>
/// Builds the real Application + Infrastructure container with NBP settings suitable for tests
/// (very short retry delays), optionally overriding individual keys.
/// </summary>
internal static class TestConfiguration
{
    public static ServiceProvider CreateServices(
        Dictionary<string, string?>? overrides = null,
        Action<IServiceCollection>? configure = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["CurrencyProviders:Nbp:BaseUrl"] = "https://api.nbp.test/api/",
            ["CurrencyProviders:Nbp:Resilience:MaxRetryAttempts"] = "3",
            ["CurrencyProviders:Nbp:Resilience:RetryBaseDelay"] = "00:00:00.001",
            ["CurrencyProviders:Nbp:Resilience:AttemptTimeout"] = "00:00:02",
            ["CurrencyProviders:Nbp:Resilience:TotalRequestTimeout"] = "00:00:10",
        };
        foreach (var (key, value) in overrides ?? [])
        {
            settings[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection()
            .AddLogging(logging => logging.SetMinimumLevel(LogLevel.None))
            .AddApplication(configuration)
            .AddInfrastructure(configuration);

        configure?.Invoke(services);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
