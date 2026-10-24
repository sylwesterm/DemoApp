using CurrencyRates.Application.Common;
using CurrencyRates.Domain.Enums;
using FluentValidation;

namespace CurrencyRates.Application.Options;

public sealed class DisplayOptionsValidator : AbstractValidator<DisplayOptions>
{
    public DisplayOptionsValidator()
    {
        RuleFor(o => o.DateFormat)
            .NotEmpty()
            .WithMessage($"Format daty ({DisplayOptions.SectionName}:{nameof(DisplayOptions.DateFormat)}) nie może być pusty.")
            .Must(BeUnambiguousDateFormat)
            .WithMessage("Format daty '{PropertyValue}' jest nieprawidłowy - musi zawierać dzień, miesiąc i rok.");

        RuleFor(o => o.CurrencySortOrder)
            .Must(BeDefinedSortOrder)
            .WithMessage("Nieprawidłowy sposób sortowania walut '{PropertyValue}'. " +
                         "Dozwolone wartości: CodeAscending, CodeDescending, NameAscending, NameDescending.");
    }

    /// <summary>
    /// A single sort order name (case-insensitive). Numbers and comma-separated combinations, which
    /// <see cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/> also accepts, are rejected.
    /// </summary>
    private static bool BeDefinedSortOrder(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !char.IsAsciiDigit(value.TrimStart()[0])
        && !value.Contains(',', StringComparison.Ordinal)
        && Enum.TryParse<CurrencySortOrder>(value, ignoreCase: true, out var sortOrder)
        && Enum.IsDefined(sortOrder);

    /// <summary>
    /// A format is accepted when it can be applied and distinguishes dates that differ
    /// only by day, only by month or only by year.
    /// </summary>
    private static bool BeUnambiguousDateFormat(string format)
    {
        var reference = new DateOnly(2001, 1, 1);

        try
        {
            var formatted = Format(reference);

            return formatted != Format(reference.AddDays(1))
                   && formatted != Format(reference.AddMonths(1))
                   && formatted != Format(reference.AddYears(1));
        }
        catch (FormatException)
        {
            return false;
        }

        string Format(DateOnly date) => date.ToString(format, DisplayCulture.PolishDates);
    }
}
