namespace CurrencyRates.Application.Exceptions;

/// <summary>
/// Thrown when an external rate provider does not respond in time. Mapped to HTTP 504.
/// </summary>
public class ExternalServiceTimeoutException : ExternalServiceException
{
    public ExternalServiceTimeoutException()
    {
    }

    public ExternalServiceTimeoutException(string message)
        : base(message)
    {
    }

    public ExternalServiceTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
