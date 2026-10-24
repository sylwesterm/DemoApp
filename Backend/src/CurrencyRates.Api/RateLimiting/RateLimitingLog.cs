namespace CurrencyRates.Api.RateLimiting;

internal static partial class RateLimitingLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Przekroczono limit zapytań: klient {ClientAddress}, żądanie {Method} {Path}")]
    public static partial void LimitExceeded(ILogger logger, string clientAddress, string method, string path);
}
