namespace CurrencyRates.Application.Features.ExchangeRates;

/// <summary>
/// Result of a conversion request: statistics and daily rates of <see cref="SourceCurrency"/>
/// expressed in <see cref="TargetCurrency"/> for the requested period.
/// </summary>
public sealed record ExchangeRateHistoryDto
{
    public required string Provider { get; init; }

    public required string SourceCurrency { get; init; }

    public required string TargetCurrency { get; init; }

    public required DateOnly DateFrom { get; init; }

    public required DateOnly DateTo { get; init; }

    public required RateStatisticsDto Statistics { get; init; }

    public required IReadOnlyList<ExchangeRateDto> Rates { get; init; }
}
