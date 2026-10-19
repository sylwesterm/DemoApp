namespace CurrencyRates.Application.Features.ExchangeRates;

public sealed record RateStatisticsDto
{
    public required decimal Minimum { get; init; }

    public required decimal Maximum { get; init; }

    public required decimal Average { get; init; }
}
