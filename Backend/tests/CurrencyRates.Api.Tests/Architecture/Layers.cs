using System.Reflection;
using CurrencyRates.Domain.Models;
using NetArchTest.Rules;

namespace CurrencyRates.Api.Tests.Architecture;

/// <summary>The four Clean Architecture layers - assemblies and root namespaces.</summary>
internal static class Layers
{
    public const string DomainNamespace = "CurrencyRates.Domain";
    public const string ApplicationNamespace = "CurrencyRates.Application";
    public const string InfrastructureNamespace = "CurrencyRates.Infrastructure";
    public const string ApiNamespace = "CurrencyRates.Api";

    public static readonly Assembly DomainAssembly = typeof(Currency).Assembly;
    public static readonly Assembly ApplicationAssembly = typeof(CurrencyRates.Application.DependencyInjection).Assembly;
    public static readonly Assembly InfrastructureAssembly = typeof(CurrencyRates.Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    public static IEnumerable<Type> AllTypes =>
        new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly }.SelectMany(assembly => assembly.GetTypes());

    /// <summary>Fails with the names of the offending types, so a broken rule says where to look.</summary>
    public static void ShouldPass(this TestResult result) =>
        result.IsSuccessful.ShouldBeTrue(
            $"Types breaking the rule: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
