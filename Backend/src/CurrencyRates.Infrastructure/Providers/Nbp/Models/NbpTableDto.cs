namespace CurrencyRates.Infrastructure.Providers.Nbp.Models;

/// <summary>
/// Response of <c>GET exchangerates/tables/{table}</c>: the latest table of average rates.
/// Only fields used by the application are declared; others are ignored.
/// </summary>
public sealed record NbpTableDto(IReadOnlyList<NbpTableRateDto> Rates);

/// <summary>One currency in a table, e.g. <c>{"currency":"dolar amerykański","code":"USD","mid":3.8974}</c>.</summary>
public sealed record NbpTableRateDto(string Currency, string Code);
