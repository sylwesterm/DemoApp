using AutoMapper;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Features.Currencies;

public sealed class CurrencyMappingProfile : Profile
{
    public CurrencyMappingProfile()
    {
        CreateMap<Currency, CurrencyDto>()
            .ForMember(dto => dto.Label, options => options.MapFrom((currency, _) => $"{currency.Code} - {currency.Name}"));
    }
}
