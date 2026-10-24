using AutoMapper;
using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Options;
using CurrencyRates.Application.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Application.Features.Currencies.GetCurrencies;

public sealed class GetCurrenciesQueryHandler(
    ICurrencyProviderFactory providerFactory,
    ICurrencySorter sorter,
    IOptions<DisplayOptions> displayOptions,
    IMapper mapper)
    : IRequestHandler<GetCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    public async Task<IReadOnlyList<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = providerFactory.GetByKey(request.ProviderKey);
        var currencies = await provider.GetCurrenciesAsync(cancellationToken);
        var sorted = sorter.Sort(currencies, displayOptions.Value.SortOrder);

        return mapper.Map<List<CurrencyDto>>(sorted);
    }
}
