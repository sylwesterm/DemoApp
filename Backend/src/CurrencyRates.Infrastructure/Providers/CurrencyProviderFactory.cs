using CurrencyRates.Application.Abstractions;
using CurrencyRates.Application.Exceptions;

namespace CurrencyRates.Infrastructure.Providers;

/// <summary>
/// Looks providers up by key among all <see cref="ICurrencyRateProvider"/> registrations,
/// so adding a provider never requires changing this class.
/// </summary>
public sealed class CurrencyProviderFactory : ICurrencyProviderFactory
{
    private readonly IReadOnlyList<ICurrencyRateProvider> providers;
    private readonly Dictionary<string, ICurrencyRateProvider> providersByKey;

    public CurrencyProviderFactory(IEnumerable<ICurrencyRateProvider> providers)
    {
        this.providers = providers.ToList();

        var duplicate = this.providers
            .GroupBy(provider => provider.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Zarejestrowano więcej niż jednego dostawcę o kluczu '{duplicate.Key}'.");
        }

        providersByKey = this.providers.ToDictionary(provider => provider.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ICurrencyRateProvider> GetAll() => providers;

    public ICurrencyRateProvider GetByKey(string key) =>
        providersByKey.TryGetValue(key ?? string.Empty, out var provider)
            ? provider
            : throw new NotFoundException($"Nie znaleziono dostawcy kursów walut '{key}'.");
}
