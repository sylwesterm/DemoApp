using CurrencyRates.Application.Common;
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
            .IsInEnum()
            .WithMessage("Nieprawidłowy sposób sortowania walut '{PropertyValue}'. " +
                         "Dozwolone wartości: CodeAscending, CodeDescending, NameAscending, NameDescending.");
    }

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

        string Format(DateOnly date) => date.ToString(format, DisplayCulture.Polish);
    }
}
