using AutoMapper;
using CurrencyRates.Application.Abstractions;
using CurrencyRates.Infrastructure.Providers.Nbp;
using CurrencyRates.Infrastructure.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_FullGraphIsValid()
    {
        // ValidateOnBuild + ValidateScopes: every service, including all MediatR handlers, can be created.
        using var services = TestConfiguration.CreateServices();
        using var scope = services.CreateScope();

        scope.ServiceProvider.GetRequiredService<ICurrencyProviderFactory>().GetByKey("NBP")
            .ShouldBeOfType<NbpCurrencyRateProvider>();
    }

    [Fact]
    public void AutoMapper_CombinesApplicationAndInfrastructureProfiles()
    {
        using var services = TestConfiguration.CreateServices();

        services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void InvalidNbpConfiguration_FailsOptionsValidation()
    {
        using var services = TestConfiguration.CreateServices(new() { ["CurrencyProviders:Nbp:BaseUrl"] = "" });

        Should.Throw<OptionsValidationException>(() => services.GetRequiredService<IOptions<NbpOptions>>().Value)
            .Message.ShouldContain("Adres API NBP");
    }
}
