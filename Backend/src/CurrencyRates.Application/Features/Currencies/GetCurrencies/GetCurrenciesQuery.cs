using MediatR;

namespace CurrencyRates.Application.Features.Currencies.GetCurrencies;

/// <summary>Lists currencies offered by a provider, sorted as configured in <c>Display:CurrencySortOrder</c>.</summary>
public sealed record GetCurrenciesQuery(string ProviderKey) : IRequest<IReadOnlyList<CurrencyDto>>;
