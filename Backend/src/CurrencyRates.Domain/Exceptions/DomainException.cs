namespace CurrencyRates.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated (e.g. an invalid currency code or date range).
/// </summary>
public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
