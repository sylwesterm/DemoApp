using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace CurrencyRates.Infrastructure.Http;

public static class ResilienceExtensions
{
    /// <summary>
    /// Adds the standard resilience pipeline (total timeout → retry → circuit breaker → attempt timeout)
    /// configured from <paramref name="settings"/>. Retries use exponential backoff with jitter, so
    /// many clients failing at once do not retry in lockstep.
    /// </summary>
    public static IHttpClientBuilder AddConfiguredResilience(
        this IHttpClientBuilder builder,
        string clientName,
        Func<IServiceProvider, HttpResilienceSettings> settings)
    {
        builder.AddStandardResilienceHandler().Configure((options, services) =>
        {
            var current = settings(services);
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("CurrencyRates.Infrastructure.Http.Resilience");

            options.TotalRequestTimeout.Timeout = current.TotalRequestTimeout;
            options.AttemptTimeout.Timeout = current.AttemptTimeout;

            options.Retry.MaxRetryAttempts = current.MaxRetryAttempts;
            options.Retry.Delay = current.RetryBaseDelay;
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.Retry.OnRetry = arguments =>
            {
                var reason = arguments.Outcome.Exception switch
                {
                    TimeoutRejectedException => $"przekroczono limit czasu próby ({current.AttemptTimeout.TotalSeconds} s)",
                    HttpRequestException => "błąd połączenia",
                    null => $"odpowiedź HTTP {(int?)arguments.Outcome.Result?.StatusCode}",
                    var exception => exception.GetType().Name,
                };
                ResilienceLog.Retrying(logger, clientName, arguments.AttemptNumber + 1, arguments.RetryDelay.TotalMilliseconds, reason);
                return default;
            };

            // The circuit breaker must observe at least two attempts to make a decision.
            var minimumSampling = current.AttemptTimeout * 2;
            if (options.CircuitBreaker.SamplingDuration < minimumSampling)
            {
                options.CircuitBreaker.SamplingDuration = minimumSampling;
            }
        });

        return builder;
    }
}
