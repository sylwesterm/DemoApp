using MediatR;

namespace CurrencyRates.Application.Features.Providers.GetProviders;

/// <summary>Lists all registered rate providers (the API drop-down).</summary>
public sealed record GetProvidersQuery : IRequest<IReadOnlyList<ProviderDto>>;
