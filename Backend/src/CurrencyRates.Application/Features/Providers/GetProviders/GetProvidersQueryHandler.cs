using AutoMapper;
using CurrencyRates.Application.Abstractions;
using MediatR;

namespace CurrencyRates.Application.Features.Providers.GetProviders;

public sealed class GetProvidersQueryHandler(ICurrencyProviderFactory providerFactory, IMapper mapper)
    : IRequestHandler<GetProvidersQuery, IReadOnlyList<ProviderDto>>
{
    public Task<IReadOnlyList<ProviderDto>> Handle(GetProvidersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProviderDto> providers = mapper.Map<List<ProviderDto>>(providerFactory.GetAll());
        return Task.FromResult(providers);
    }
}
