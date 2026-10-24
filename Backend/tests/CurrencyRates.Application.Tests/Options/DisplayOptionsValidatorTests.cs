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
    [InlineData("dd/MM/yyyy")]
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

    [Theory]
    [InlineData("NameDescending", CurrencySortOrder.NameDescending)]
    [InlineData("codeascending", CurrencySortOrder.CodeAscending)]
    public void CurrencySortOrder_KnownName_IsAcceptedCaseInsensitively(string value, CurrencySortOrder expected)
    {
        var options = new DisplayOptions { CurrencySortOrder = value };

        validator.TestValidate(options).ShouldNotHaveAnyValidationErrors();
        options.SortOrder.ShouldBe(expected);
    }

    [Theory]
    [InlineData("CodeAsc")]  // a typo - the configuration binder would reject it with an English message
    [InlineData("7")]
    [InlineData("1")]        // numbers are not documented values, even when they map to one
    [InlineData("CodeAscending, NameDescending")]  // Enum.TryParse would combine them into a defined value
    [InlineData("")]
    public void CurrencySortOrder_Unknown_ReportsPolishError(string value)
    {
        validator.TestValidate(new DisplayOptions { CurrencySortOrder = value })
            .ShouldHaveValidationErrorFor(o => o.CurrencySortOrder)
            .WithErrorMessage($"Nieprawidłowy sposób sortowania walut '{value}'. " +
                              "Dozwolone wartości: CodeAscending, CodeDescending, NameAscending, NameDescending.");
    }
}
