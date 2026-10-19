namespace CurrencyRates.Application.Features.Providers;

public sealed record ProviderDto
{
    public required string Key { get; init; }

    public required string DisplayName { get; init; }

    public required string BaseCurrencyCode { get; init; }
}
