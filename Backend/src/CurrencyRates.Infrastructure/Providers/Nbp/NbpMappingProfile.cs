using AutoMapper;
using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp.Models;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Maps NBP JSON DTOs to domain models through their constructors, so domain invariants are enforced.
/// </summary>
public sealed class NbpMappingProfile : Profile
{
    public NbpMappingProfile()
    {
        CreateMap<NbpTableRateDto, Currency>()
            .ForCtorParam("code", options => options.MapFrom(dto => dto.Code))
            .ForCtorParam("name", options => options.MapFrom(dto => dto.Currency));

        CreateMap<NbpRateDto, ExchangeRate>()
            .ForCtorParam("date", options => options.MapFrom(dto => dto.EffectiveDate))
            .ForCtorParam("value", options => options.MapFrom(dto => dto.Mid));
    }
}
