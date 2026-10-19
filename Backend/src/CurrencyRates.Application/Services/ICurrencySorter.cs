using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Services;

public interface ICurrencySorter
{
    IReadOnlyList<Currency> Sort(IEnumerable<Currency> currencies, CurrencySortOrder order);
}
