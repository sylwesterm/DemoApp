using CurrencyRates.Application.Options;
using CurrencyRates.Domain.Enums;
using FluentValidation.TestHelper;

namespace CurrencyRates.Application.Tests.Options;

public class DisplayOptionsValidatorTests
{
    private readonly DisplayOptionsValidator validator = new();

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("dd.MM.yyyy")]
    [InlineData("d MMMM yyyy")]
    public void DateFormat_Valid_HasNoErrors(string format)
    {
        validator.TestValidate(new DisplayOptions { DateFormat = format }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DateFormat_Empty_ReportsPolishError()
    {
        validator.TestValidate(new DisplayOptions { DateFormat = "" })
            .ShouldHaveValidationErrorFor(o => o.DateFormat)
            .WithErrorMessage("Format daty (Display:DateFormat) nie może być pusty.");
    }

    [Theory]
    [InlineData("yyyy-MM")]  // no day
    [InlineData("dd.yyyy")]  // no month
    [InlineData("dd.MM")]    // no year
    [InlineData("%")]        // throws FormatException
    public void DateFormat_Ambiguous_ReportsPolishError(string format)
    {
        validator.TestValidate(new DisplayOptions { DateFormat = format })
            .ShouldHaveValidationErrorFor(o => o.DateFormat)
            .WithErrorMessage($"Format daty '{format}' jest nieprawidłowy - musi zawierać dzień, miesiąc i rok.");
    }

    [Fact]
    public void CurrencySortOrder_UndefinedValue_ReportsPolishError()
    {
        validator.TestValidate(new DisplayOptions { CurrencySortOrder = (CurrencySortOrder)7 })
            .ShouldHaveValidationErrorFor(o => o.CurrencySortOrder)
            .WithErrorMessage("Nieprawidłowy sposób sortowania walut '7'. " +
                              "Dozwolone wartości: CodeAscending, CodeDescending, NameAscending, NameDescending.");
    }
}
