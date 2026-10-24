namespace CurrencyRates.Infrastructure.Providers.Nbp.Models;

/// <summary>
/// Response of <c>GET exchangerates/rates/{table}/{code}/{from}/{to}</c>: daily rates of one currency.
/// Only fields used by the application are declared; others are ignored.
/// </summary>
public sealed record NbpRateSeriesDto(IReadOnlyList<NbpRateDto> Rates);

/// <summary>Rate on one day, e.g. <c>{"no":"201/A/NBP/2026","effectiveDate":"2026-10-15","mid":4.3691}</c>.</summary>
public sealed record NbpRateDto(DateOnly EffectiveDate, decimal Mid);
