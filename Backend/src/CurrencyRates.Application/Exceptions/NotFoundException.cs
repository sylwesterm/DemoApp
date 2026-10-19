namespace CurrencyRates.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource (provider, currency) does not exist. Mapped to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
