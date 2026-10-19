using CurrencyRates.Domain.Exceptions;

namespace CurrencyRates.Domain.Models;

/// <summary>
/// An inclusive range of calendar days.
/// </summary>
public sealed record DateRange
{
    public DateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new DomainException($"Data początkowa {from:yyyy-MM-dd} jest późniejsza niż data końcowa {to:yyyy-MM-dd}.");
        }

        From = from;
        To = to;
    }

    public DateOnly From { get; }

    public DateOnly To { get; }

    public int DayCount => To.DayNumber - From.DayNumber + 1;

    public static DateRange SingleDay(DateOnly day) => new(day, day);

    public bool Contains(DateOnly date) => date >= From && date <= To;
}
