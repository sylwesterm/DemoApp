namespace CurrencyRates.Application.Abstractions;

/// <summary>
/// Gives access to the registered <see cref="ICurrencyRateProvider"/> implementations.
/// </summary>
public interface ICurrencyProviderFactory
{
    /// <summary>Returns all registered providers.</summary>
    IReadOnlyList<ICurrencyRateProvider> GetAll();

    /// <summary>Returns the provider with the given key (case-insensitive).</summary>
    /// <exception cref="Exceptions.NotFoundException">No provider with that key is registered.</exception>
    ICurrencyRateProvider GetByKey(string key);
}
