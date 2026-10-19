using CurrencyRates.Domain.Exceptions;

namespace CurrencyRates.Domain.Models;

/// <summary>
/// The value of a currency on a given day, expressed in some reference currency.
/// </summary>
public sealed record ExchangeRate
{
    public ExchangeRate(DateOnly date, decimal value)
    {
        if (value <= 0)
        {
            throw new DomainException($"Kurs z dnia {date:yyyy-MM-dd} musi być dodatni, a wynosi {value}.");
        }

        Date = date;
        Value = value;
    }

    public DateOnly Date { get; }

    public decimal Value { get; }
}
