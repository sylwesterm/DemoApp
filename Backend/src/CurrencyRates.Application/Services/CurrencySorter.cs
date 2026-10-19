using CurrencyRates.Application.Common;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Sorts currencies for the drop-down lists. Names are compared using Polish rules, so
/// "łari gruzińskie" comes between "lej rumuński" and "manat" (an ordinal comparison would put it after "z").
/// Codes are compared ordinally; ties are broken by code so the order is stable.
/// </summary>
public sealed class CurrencySorter : ICurrencySorter
{
    private static readonly StringComparer NameComparer = StringComparer.Create(DisplayCulture.Polish, ignoreCase: true);

    public IReadOnlyList<Currency> Sort(IEnumerable<Currency> currencies, CurrencySortOrder order)
    {
        ArgumentNullException.ThrowIfNull(currencies);

        var sorted = order switch
        {
            CurrencySortOrder.CodeAscending => currencies.OrderBy(c => c.Code, StringComparer.Ordinal),
            CurrencySortOrder.CodeDescending => currencies.OrderByDescending(c => c.Code, StringComparer.Ordinal),
            CurrencySortOrder.NameAscending => currencies.OrderBy(c => c.Name, NameComparer)
                .ThenBy(c => c.Code, StringComparer.Ordinal),
            CurrencySortOrder.NameDescending => currencies.OrderByDescending(c => c.Name, NameComparer)
                .ThenBy(c => c.Code, StringComparer.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, "Nieobsługiwany sposób sortowania walut."),
        };

        return sorted.ToList();
    }
}
