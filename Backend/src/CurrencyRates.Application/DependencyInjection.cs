using CurrencyRates.Application.Behaviors;
using CurrencyRates.Application.Common;
using CurrencyRates.Application.Options;
using CurrencyRates.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CurrencyRates.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the application layer: CQRS handlers and pipeline, options, validators,
    /// calculation services and mapping profiles.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Behaviors run in registration order: logging wraps validation, which wraps the handler.
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Validators are stateless, so a single instance is shared (also required by options validation).
        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton);

        services.AddOptions<DisplayOptions>()
            .Bind(configuration.GetSection(DisplayOptions.SectionName))
            .ValidateWithFluentValidation()
            .ValidateOnStart();

        // Tests replace it with a fake clock to control "today".
        services.TryAddSingleton(TimeProvider.System);

        // Stateless calculation services.
        services.AddSingleton<ICrossRateCalculator, CrossRateCalculator>();
        services.AddSingleton<IRateStatisticsCalculator, RateStatisticsCalculator>();
        services.AddSingleton<ICurrencySorter, CurrencySorter>();

        // The license key is optional (AutoMapper only logs a warning without it) and must not be
        // committed - provide it via user secrets or the AutoMapper__LicenseKey environment variable.
        services.AddAutoMapper(
            config => config.LicenseKey = configuration["AutoMapper:LicenseKey"],
            assembly);

        return services;
    }
}
