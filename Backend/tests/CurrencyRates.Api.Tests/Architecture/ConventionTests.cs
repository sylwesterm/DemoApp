using System.Reflection;
using CurrencyRates.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static CurrencyRates.Api.Tests.Architecture.Layers;

namespace CurrencyRates.Api.Tests.Architecture;

/// <summary>Conventions the design relies on: thin controllers, CQRS shape, immutable domain, pluggable providers.</summary>
public sealed class ConventionTests
{
    [Fact]
    public void Controllers_AreSealedAndDependOnlyOnSender()
    {
        var controllers = ApiAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ControllerBase))).ToList();

        controllers.ShouldNotBeEmpty();
        controllers.ShouldAllBe(type => type.IsSealed && type.Name.EndsWith("Controller", StringComparison.Ordinal));
        // Thin controllers: everything else is reached through MediatR - and only the Send side of it.
        controllers.ShouldAllBe(type => type.GetConstructors().All(constructor =>
            constructor.GetParameters().All(parameter => parameter.ParameterType == typeof(ISender))));
    }

    [Fact]
    public void Requests_AreImmutableRecordsNamedAsQueriesOrCommands()
    {
        var requests = ApplicationAssembly.GetTypes().Where(type => Implements(type, typeof(IRequest<>))).ToList();

        requests.ShouldNotBeEmpty();
        requests.ShouldAllBe(type =>
            type.IsSealed
            && IsRecord(type)
            && (type.Name.EndsWith("Query", StringComparison.Ordinal) || type.Name.EndsWith("Command", StringComparison.Ordinal)));
        requests.ShouldAllBe(type => HasNoPublicSetters(type));
    }

    [Fact]
    public void RequestHandlers_AreSealedAndNamedAfterTheirRequest()
    {
        var handlers = ApplicationAssembly.GetTypes().Where(type => Implements(type, typeof(IRequestHandler<,>))).ToList();

        handlers.ShouldNotBeEmpty();
        handlers.ShouldAllBe(type => type.IsSealed);
        handlers.ShouldAllBe(type => type.Name == HandledRequest(type).Name + "Handler");
    }

    [Fact]
    public void Validators_AreSealedAndNamedAfterTheValidatedType()
    {
        var validators = AllTypes
            .Where(type => type is { IsAbstract: false, BaseType.IsGenericType: true }
                && type.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        validators.ShouldNotBeEmpty();
        validators.ShouldAllBe(type =>
            type.IsSealed && type.Name == type.BaseType!.GetGenericArguments()[0].Name + "Validator");
    }

    [Fact]
    public void DomainModels_AreSealedAndImmutable()
    {
        var models = DomainAssembly.GetTypes()
            .Where(type => type.Namespace == $"{DomainNamespace}.Models" && type is { IsClass: true, IsPublic: true })
            .ToList();

        models.ShouldNotBeEmpty();
        models.ShouldAllBe(type => type.IsSealed && HasNoPublicSetters(type));
    }

    [Fact]
    public void RateProviders_AreImplementedInInfrastructure()
    {
        // A new provider is added by implementing ICurrencyRateProvider in Infrastructure - nothing else changes.
        var providers = AllTypes
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ICurrencyRateProvider).IsAssignableFrom(type))
            .ToList();

        providers.ShouldNotBeEmpty();
        providers.ShouldAllBe(type =>
            type.Assembly == InfrastructureAssembly && type.IsSealed && type.Namespace!.StartsWith($"{InfrastructureNamespace}.Providers", StringComparison.Ordinal));
    }

    private static bool Implements(Type type, Type genericInterface) =>
        type is { IsClass: true, IsAbstract: false }
        && type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == genericInterface);

    private static Type HandledRequest(Type handler) =>
        handler.GetInterfaces()
            .First(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .GetGenericArguments()[0];

    /// <summary>The compiler marks records with a synthesized <c>EqualityContract</c> property.</summary>
    private static bool IsRecord(Type type) =>
        type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic) is not null;

    /// <summary>Only <c>get</c> or <c>init</c> accessors - <c>init</c> is marked with the IsExternalInit modifier.</summary>
    private static bool HasNoPublicSetters(Type type) =>
        type.GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property =>
            property.SetMethod is not { IsPublic: true } setter
            || setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier => modifier.Name == "IsExternalInit"));
}
