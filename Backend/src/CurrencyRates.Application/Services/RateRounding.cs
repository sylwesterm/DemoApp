namespace CurrencyRates.Application.Services;

/// <summary>
/// Single rounding rule for calculated rates, so cross rates and statistics are consistent.
/// </summary>
public static class RateRounding
{
    /// <summary>
    /// 8 decimal places keep meaningful precision even for very small cross rates
    /// (e.g. 1 IDR ≈ 0.0000551 EUR), while NBP itself publishes 4-6 places.
    /// </summary>
    public const int DecimalPlaces = 8;

    public static decimal Round(decimal value) =>
        Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);
}
