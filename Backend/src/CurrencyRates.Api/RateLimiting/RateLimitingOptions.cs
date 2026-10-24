namespace CurrencyRates.Api.RateLimiting;

/// <summary>
/// Limit of API requests per client IP address, bound from the <c>RateLimiting</c> configuration section.
/// Protects the upstream providers: every rates request may trigger calls to the provider's API.
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Requests one client may make within <see cref="Window"/>.</summary>
    public int PermitLimit { get; set; } = 60;

    /// <summary>Length of the fixed window after which the limit is renewed.</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}
