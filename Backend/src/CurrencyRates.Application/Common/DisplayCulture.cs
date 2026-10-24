using System.Globalization;

namespace CurrencyRates.Application.Common;

/// <summary>
/// Culture used for everything presented to the user (date formatting, sorting by name).
/// </summary>
public static class DisplayCulture
{
    public static CultureInfo Polish { get; } = CultureInfo.GetCultureInfo("pl-PL");

    /// <summary>
    /// Polish culture for applying the configured <c>Display:DateFormat</c> literally: month and day names stay
    /// Polish, but <c>/</c> - in .NET a placeholder for the culture's date separator, which is <c>.</c> in pl-PL -
    /// stays <c>/</c>, so <c>dd/MM/yyyy</c> gives <c>16/10/2026</c> as configured.
    /// </summary>
    public static CultureInfo PolishDates { get; } = CreatePolishDates();

    private static CultureInfo CreatePolishDates()
    {
        var culture = (CultureInfo)Polish.Clone();
        culture.DateTimeFormat.DateSeparator = "/";
        return CultureInfo.ReadOnly(culture);
    }
}
