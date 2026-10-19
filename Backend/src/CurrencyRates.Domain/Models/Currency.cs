using CurrencyRates.Domain.Exceptions;

namespace CurrencyRates.Domain.Models;

/// <summary>
/// A currency identified by its ISO 4217 code, e.g. <c>CAD - dolar kanadyjski</c>.
/// The code is the identity: two currencies with the same code are equal regardless of name.
/// </summary>
public sealed record Currency
{
    private const int CodeLength = 3;

    public Currency(string code, string name)
    {
        Code = NormalizeCode(code);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Waluta '{Code}' musi mieć nazwę.");
        }

        Name = name.Trim();
    }

    public string Code { get; }

    public string Name { get; }

    public bool Equals(Currency? other) => other is not null && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Validates a currency code and returns it trimmed and upper-cased.
    /// </summary>
    public static string NormalizeCode(string code)
    {
        var normalized = code?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalized.Length != CodeLength || !normalized.All(char.IsAsciiLetterUpper))
        {
            throw new DomainException($"'{code}' nie jest prawidłowym trzyliterowym kodem waluty.");
        }

        return normalized;
    }
}
