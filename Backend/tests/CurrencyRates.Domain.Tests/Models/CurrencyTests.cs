using CurrencyRates.Domain.Exceptions;
using CurrencyRates.Domain.Models;

namespace CurrencyRates.Domain.Tests.Models;

public class CurrencyTests
{
    [Theory]
    [InlineData("eur", "EUR")]
    [InlineData(" Usd ", "USD")]
    [InlineData("CHF", "CHF")]
    public void Constructor_ValidCode_NormalizesToTrimmedUpperCase(string code, string expected)
    {
        var currency = new Currency(code, "waluta");

        currency.Code.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    [InlineData("ŁÓD")]
    public void Constructor_InvalidCode_ThrowsDomainException(string code)
    {
        Should.Throw<DomainException>(() => new Currency(code, "waluta"))
            .Message.ShouldContain("nie jest prawidłowym trzyliterowym kodem waluty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_EmptyName_ThrowsDomainException(string name)
    {
        Should.Throw<DomainException>(() => new Currency("EUR", name))
            .Message.ShouldBe("Waluta 'EUR' musi mieć nazwę.");
    }

    [Fact]
    public void Constructor_NameWithSpaces_IsTrimmed()
    {
        new Currency("EUR", "  euro ").Name.ShouldBe("euro");
    }

    [Fact]
    public void Equality_SameCodeDifferentName_AreEqual()
    {
        var first = new Currency("usd", "dolar amerykański");
        var second = new Currency("USD", "Dolar Amerykański");

        first.ShouldBe(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        new HashSet<Currency> { first, second }.Count.ShouldBe(1);
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        new Currency("EUR", "euro").ShouldNotBe(new Currency("USD", "euro"));
    }
}
