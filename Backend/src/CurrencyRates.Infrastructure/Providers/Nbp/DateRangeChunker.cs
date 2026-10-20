using CurrencyRates.Domain.Models;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

/// <summary>
/// Splits a date range into consecutive, non-overlapping chunks no longer than the API allows per request.
/// </summary>
public static class DateRangeChunker
{
    public static IReadOnlyList<DateRange> Split(DateRange range, int maxDaysPerChunk)
    {
        ArgumentNullException.ThrowIfNull(range);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDaysPerChunk, 1);

        var chunks = new List<DateRange>();
        for (var start = range.From; start <= range.To; start = start.AddDays(maxDaysPerChunk))
        {
            var end = start.AddDays(maxDaysPerChunk - 1);
            chunks.Add(new DateRange(start, end < range.To ? end : range.To));
        }

        return chunks;
    }
}
