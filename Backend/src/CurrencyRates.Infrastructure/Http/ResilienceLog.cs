using Microsoft.Extensions.Logging;

namespace CurrencyRates.Infrastructure.Http;

internal static partial class ResilienceLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Ponawianie wywołania {Client} (próba {Attempt}) za {DelayMilliseconds:F0} ms. Przyczyna: {Reason}")]
    public static partial void Retrying(ILogger logger, string client, int attempt, double delayMilliseconds, string reason);
}
