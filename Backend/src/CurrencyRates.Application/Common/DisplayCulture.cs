using System.Globalization;

namespace CurrencyRates.Application.Common;

/// <summary>
/// Culture used for everything presented to the user (date formatting, sorting by name).
/// </summary>
public static class DisplayCulture
{
    public static CultureInfo Polish { get; } = CultureInfo.GetCultureInfo("pl-PL");
}
