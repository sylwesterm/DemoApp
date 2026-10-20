using CurrencyRates.Application.Services;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Application.Tests.Services;

public class CurrencySorterTests
{
    private static readonly Currency[] Currencies =
    [
        new("MDL", "lej mołdawski"),
        new("GEL", "łari gruzińskie"),
        new("AZN", "manat azerbejdżański"),
        new("PLN", "złoty polski"),
    ];

    private readonly CurrencySorter sorter = new();

    [Theory]
    [InlineData(CurrencySortOrder.CodeAscending, new[] { "AZN", "GEL", "MDL", "PLN" })]
    [InlineData(CurrencySortOrder.CodeDescending, new[] { "PLN", "MDL", "GEL", "AZN" })]
    [InlineData(CurrencySortOrder.NameAscending, new[] { "MDL", "GEL", "AZN", "PLN" })]
    [InlineData(CurrencySortOrder.NameDescending, new[] { "PLN", "AZN", "GEL", "MDL" })]
    public void Sort_EachOrder_ReturnsExpectedSequence(CurrencySortOrder order, string[] expectedCodes)
    {
        sorter.Sort(Currencies, order).Select(c => c.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public void Sort_ByName_UsesPolishAlphabet()
    {
        // Ordinal comparison would put "ł" after "z"; Polish rules put it right after "l".
        var sorted = sorter.Sort(Currencies, CurrencySortOrder.NameAscending).Select(c => c.Name).ToList();

        sorted.IndexOf("łari gruzińskie").ShouldBe(sorted.IndexOf("lej mołdawski") + 1);
    }

    [Fact]
    public void Sort_ByNameWithEqualNames_BreaksTieByCode()
    {
        Currency[] sameName = [new("ZZZ", "dolar"), new("AAA", "dolar")];

        sorter.Sort(sameName, CurrencySortOrder.NameAscending).Select(c => c.Code).ShouldBe(["AAA", "ZZZ"]);
        sorter.Sort(sameName, CurrencySortOrder.NameDescending).Select(c => c.Code).ShouldBe(["AAA", "ZZZ"]);
    }

    [Fact]
    public void Sort_UndefinedOrder_ThrowsArgumentOutOfRange()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => sorter.Sort(Currencies, (CurrencySortOrder)99));
    }
}
