using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Application.Behaviors;

/// <summary>
/// Logs every request with its duration. Failures are not logged here - the API's global
/// exception handler logs them once, together with the HTTP status returned.
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        LogHandling(requestName, new JsonText(request));

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);

        LogHandled(requestName, stopwatch.ElapsedMilliseconds);
        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Obsługa zapytania {RequestName}: {Request}")]
    private partial void LogHandling(string requestName, JsonText request);

    /// <summary>
    /// The request as JSON, produced only when the entry is actually written (the logger calls ToString then).
    /// JSON rather than structured destructuring: dates come out as ISO (2026-10-12) on every machine, whereas
    /// the logging sinks would render DateOnly in the machine's culture (12/10/2026).
    /// </summary>
    private readonly struct JsonText(object value)
    {
        public override string ToString() => JsonSerializer.Serialize(value, value.GetType());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Zakończono obsługę zapytania {RequestName} w {ElapsedMilliseconds} ms")]
    private partial void LogHandled(string requestName, long elapsedMilliseconds);
}
