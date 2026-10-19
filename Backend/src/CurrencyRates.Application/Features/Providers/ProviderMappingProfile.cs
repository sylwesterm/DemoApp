using AutoMapper;
using CurrencyRates.Application.Abstractions;

namespace CurrencyRates.Application.Features.Providers;

public sealed class ProviderMappingProfile : Profile
{
    public ProviderMappingProfile()
    {
        CreateMap<ICurrencyRateProvider, ProviderDto>();
    }
}
