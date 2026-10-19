using AutoMapper;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Features.ExchangeRates;

public sealed class ExchangeRateMappingProfile : Profile
{
    public ExchangeRateMappingProfile()
    {
        CreateMap<ExchangeRate, ExchangeRateDto>()
            .ForMember(dto => dto.FormattedDate, options => options.MapFrom<FormattedDateResolver>());

        CreateMap<RateStatistics, RateStatisticsDto>();
    }
}
