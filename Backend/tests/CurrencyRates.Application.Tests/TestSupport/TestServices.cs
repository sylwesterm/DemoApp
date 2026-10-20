using AutoMapper;
using CurrencyRates.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Application.Tests.TestSupport;

/// <summary>
/// Builds the real application-layer container, so tests use the production AutoMapper
/// configuration, options binding and MediatR pipeline instead of hand-made copies.
/// </summary>
internal static class TestServices
{
    public static ServiceProvider Create(
        string dateFormat = "yyyy-MM-dd",
        CurrencySortOrder sortOrder = CurrencySortOrder.CodeAscending,
        Action<IServiceCollection>? configure = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Display:DateFormat"] = dateFormat,
                ["Display:CurrencySortOrder"] = sortOrder.ToString(),
            })
            .Build();

        var services = new ServiceCollection()
            .AddLogging(logging => logging.SetMinimumLevel(LogLevel.None))
            .AddApplication(configuration);

        configure?.Invoke(services);

        // Infrastructure services (e.g. the provider factory) are only present when a test adds them,
        // so the whole graph is not validated on build - only scope rules are.
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public static IMapper CreateMapper(string dateFormat = "yyyy-MM-dd") =>
        Create(dateFormat).GetRequiredService<IMapper>();
}
