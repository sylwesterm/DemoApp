using NetArchTest.Rules;
using static CurrencyRates.Api.Tests.Architecture.Layers;

namespace CurrencyRates.Api.Tests.Architecture;

/// <summary>
/// Clean Architecture: dependencies point inward (Api → Infrastructure → Application → Domain),
/// and frameworks stay at the edges, so the inner layers can be reused and tested without them.
/// </summary>
public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotDependOnOtherLayers() =>
        Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Domain_DoesNotDependOnAnyFramework() =>
        Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "MediatR",
                "AutoMapper",
                "FluentValidation",
                "Serilog",
                "Polly",
                "Microsoft.Extensions",
                "Microsoft.AspNetCore",
                "System.Net.Http")
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi() =>
        Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Application_DoesNotDependOnWebHttpOrLoggingImplementation() =>
        Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Microsoft.AspNetCore",
                "System.Net.Http",
                "Microsoft.Extensions.Http",
                "Polly",
                "Serilog")
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Infrastructure_DoesNotDependOnApi() =>
        Types.InAssembly(InfrastructureAssembly)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Infrastructure_DoesNotDependOnAspNetCoreOrSerilog() =>
        Types.InAssembly(InfrastructureAssembly)
            .ShouldNot().HaveDependencyOnAny("Microsoft.AspNetCore", "Serilog")
            .GetResult()
            .ShouldPass();

    [Fact]
    public void Controllers_DoNotDependOnInfrastructureOrDomain() =>
        Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace($"{ApiNamespace}.Controllers")
            .ShouldNot().HaveDependencyOnAny(InfrastructureNamespace, DomainNamespace)
            .GetResult()
            .ShouldPass();
}
