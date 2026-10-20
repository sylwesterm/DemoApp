using CurrencyRates.Infrastructure.Http;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Settings of the NBP provider, bound from <c>CurrencyProviders:Nbp</c>.
/// </summary>
public sealed class NbpOptions
{
    public const string SectionName = "CurrencyProviders:Nbp";

    public string DisplayName { get; set; } = "Narodowy Bank Polski";

    /// <summary>API root, e.g. <c>https://api.nbp.pl/api/</c> (must end with a slash).</summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>Longest date range NBP accepts in one request; longer ranges are split.</summary>
    public int MaxDaysPerRequest { get; set; } = 367;

    /// <summary>How long the currency list (tables A and B) is cached.</summary>
    public TimeSpan CurrencyListCacheDuration { get; set; } = TimeSpan.FromHours(1);

    public HttpResilienceSettings Resilience { get; set; } = new();
}
