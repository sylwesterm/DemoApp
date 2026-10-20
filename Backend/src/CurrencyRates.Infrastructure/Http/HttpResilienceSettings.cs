namespace CurrencyRates.Infrastructure.Http;

/// <summary>
/// Resilience settings of an outgoing HTTP client, shared by all providers.
/// Retries use exponential backoff with jitter.
/// </summary>
public sealed class HttpResilienceSettings
{
    /// <summary>How many times a failed call (5xx, 408, 429, network error, timeout) is retried.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Delay before the first retry; doubles with each attempt and is randomized (jitter).</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Time limit of a single attempt.</summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Time limit of the whole call, including all retries.</summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
