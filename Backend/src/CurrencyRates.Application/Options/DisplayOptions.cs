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

    /// <summary>
    /// Order of currencies in the drop-down lists, as written in the configuration (case-insensitive).
    /// Bound as text, so that a typo is reported by <see cref="DisplayOptionsValidator"/> with a Polish message
    /// instead of failing in the configuration binder with an English one.
    /// </summary>
    public string CurrencySortOrder { get; set; } = nameof(Domain.Enums.CurrencySortOrder.CodeAscending);

    /// <summary>The parsed <see cref="CurrencySortOrder"/>; valid once the options have passed validation.</summary>
    public CurrencySortOrder SortOrder => Enum.Parse<CurrencySortOrder>(CurrencySortOrder, ignoreCase: true);
}
