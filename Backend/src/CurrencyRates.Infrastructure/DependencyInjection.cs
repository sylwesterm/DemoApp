using System.Net.Http.Headers;
using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Common;
using CurrencyRates.Infrastructure.Http;
using CurrencyRates.Infrastructure.Providers;
using CurrencyRates.Infrastructure.Providers.Nbp;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers rate providers and their HTTP clients. To add a provider, implement
    /// <see cref="ICurrencyRateProvider"/> and register it here - it appears in the API automatically.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton);
        services.AddMemoryCache();

        // Adds this assembly's profiles to the configuration created in AddApplication().
        services.AddAutoMapper(_ => { }, assembly);

        services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();
        services.AddNbpProvider(configuration);

        return services;
    }

    private static void AddNbpProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NbpOptions>()
            .Bind(configuration.GetSection(NbpOptions.SectionName))
            .ValidateWithFluentValidation()
            .ValidateOnStart();

        services.AddHttpClient<INbpApiClient, NbpApiClient>((provider, client) =>
            {
                client.BaseAddress = provider.GetRequiredService<IOptions<NbpOptions>>().Value.BaseUrl;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddConfiguredResilience(
                "NBP",
                provider => provider.GetRequiredService<IOptions<NbpOptions>>().Value.Resilience);

        // Scoped, not singleton: the typed HttpClient must not be captured for the app's lifetime.
        services.AddScoped<ICurrencyRateProvider, NbpCurrencyRateProvider>();
    }
}
