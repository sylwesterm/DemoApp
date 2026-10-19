namespace CurrencyRates.Application.Features.ExchangeRates;

/// <summary>
/// Value of one unit of the source currency in the target currency on a given day.
/// </summary>
public sealed record ExchangeRateDto
{
    /// <summary>ISO date (<c>yyyy-MM-dd</c>), for sorting and charts.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Date formatted with the configured <c>Display:DateFormat</c>, for the results table.</summary>
    public required string FormattedDate { get; init; }

    public required decimal Value { get; init; }
}
