namespace CurrencyRates.Application.Exceptions;

/// <summary>
/// Thrown when an external rate provider fails or returns invalid data. Mapped to HTTP 502.
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException()
    {
    }

    public ExternalServiceException(string message)
        : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
