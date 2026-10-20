using CurrencyRates.Domain.Models;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Currencies offered by NBP together with the table (A or B) each one is published in.
/// </summary>
public sealed record NbpCurrencyCatalog(IReadOnlyList<Currency> Currencies, IReadOnlyDictionary<string, string> TableByCode);
