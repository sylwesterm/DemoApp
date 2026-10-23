namespace CurrencyRates.Api.ErrorHandling;

internal static partial class ErrorHandlingLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Odrzucono żądanie {Method} {Path} z kodem {StatusCode}: {Reason}")]
    public static partial void RequestRejected(ILogger logger, string method, string? path, int statusCode, string reason);

    /// <summary>
    /// Like <see cref="RequestRejected"/>, but with the stack trace: a domain rule broken past the validators
    /// may point to a bug, so the developer needs to see where it was thrown.
    /// </summary>
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Odrzucono żądanie {Method} {Path} z kodem {StatusCode} (naruszona reguła domeny): {Reason}")]
    public static partial void DomainRuleViolated(ILogger logger, Exception exception, string method, string? path, int statusCode, string reason);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Błąd dostawcy kursów walut podczas żądania {Method} {Path} (kod {StatusCode}): {Reason}")]
    public static partial void ProviderFailed(ILogger logger, Exception exception, string method, string? path, int statusCode, string reason);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Nieobsłużony wyjątek podczas żądania {Method} {Path}")]
    public static partial void UnhandledException(ILogger logger, Exception exception, string method, string? path);
}
