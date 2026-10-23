using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace CurrencyRates.Api.Tests.TestSupport;

/// <summary>Collects Serilog events in memory so tests can assert what was logged, at which level and with which exception.</summary>
internal sealed class CapturingSink : ILogEventSink
{
    /// <summary>Source of the error handler's entries; the class itself is internal to the API.</summary>
    public const string ExceptionHandler = "CurrencyRates.Api.ErrorHandling.GlobalExceptionHandler";

    private readonly ConcurrentQueue<LogEvent> _events = new();

    public IReadOnlyList<LogEvent> All => [.. _events];

    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    public IReadOnlyList<LogEvent> From(string sourceContext) =>
        _events.Where(logEvent =>
                logEvent.Properties.TryGetValue("SourceContext", out var source)
                && source is ScalarValue { Value: string name }
                && name == sourceContext)
            .ToList();
}
