namespace CurrencyRates.Api.ErrorHandling;

internal static class LogText
{
    /// <summary>
    /// Replaces control characters (CR, LF, ...) with spaces. Messages may echo values sent by the client
    /// (e.g. <c>dateFrom</c> or the provider key), which must not be able to forge extra lines in the log file.
    /// </summary>
    public static string SingleLine(string text) =>
        string.Create(text.Length, text, static (buffer, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                buffer[i] = char.IsControl(source[i]) ? ' ' : source[i];
            }
        });
}
