using CurrencyRates.Domain.Models;
using CurrencyRates.Infrastructure.Providers.Nbp;

namespace CurrencyRates.Infrastructure.Tests.Providers.Nbp;

public class DateRangeChunkerTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);

    [Fact]
    public void Split_RangeWithinLimit_ReturnsSingleChunk()
    {
        var range = new DateRange(Start, Start.AddDays(366));

        DateRangeChunker.Split(range, 367).ShouldHaveSingleItem().ShouldBe(range);
    }

    [Fact]
    public void Split_RangeExactlyTwiceTheLimit_ReturnsTwoFullChunks()
    {
        var chunks = DateRangeChunker.Split(new DateRange(Start, Start.AddDays(19)), 10);

        chunks.ShouldBe([new DateRange(Start, Start.AddDays(9)), new DateRange(Start.AddDays(10), Start.AddDays(19))]);
    }

    [Fact]
    public void Split_RangeLongerThanLimit_LastChunkIsShorter()
    {
        var chunks = DateRangeChunker.Split(new DateRange(Start, Start.AddDays(24)), 10);

        chunks.Select(c => c.DayCount).ShouldBe([10, 10, 5]);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 7)]
    [InlineData(800, 367)]
    public void Split_ChunksAreContiguousAndCoverWholeRange(int days, int maxDays)
    {
        var range = new DateRange(Start, Start.AddDays(days - 1));

        var chunks = DateRangeChunker.Split(range, maxDays);

        chunks[0].From.ShouldBe(range.From);
        chunks[^1].To.ShouldBe(range.To);
        chunks.Sum(c => c.DayCount).ShouldBe(range.DayCount);
        chunks.ShouldAllBe(c => c.DayCount <= maxDays);
        chunks.Zip(chunks.Skip(1)).ShouldAllBe(pair => pair.Second.From == pair.First.To.AddDays(1));
    }

    [Fact]
    public void Split_NonPositiveLimit_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => DateRangeChunker.Split(DateRange.SingleDay(Start), 0));
    }
}
