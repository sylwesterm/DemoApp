using CurrencyRates.Domain.Enums;

namespace CurrencyRates.Application.Options;

/// <summary>
/// Presentation settings bound from the <c>Display</c> configuration section.
/// </summary>
public sealed class DisplayOptions
{
    public const string SectionName = "Display";

    /// <summary>.NET date format used in the results table, e.g. <c>yyyy-MM-dd</c>.</summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>Order of currencies in the drop-down lists.</summary>
    public CurrencySortOrder CurrencySortOrder { get; set; } = CurrencySortOrder.CodeAscending;
}
