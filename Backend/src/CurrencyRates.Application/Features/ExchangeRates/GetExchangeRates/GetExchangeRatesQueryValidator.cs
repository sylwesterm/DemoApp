using CurrencyRates.Application.Common;
using FluentValidation;

namespace CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;

public sealed class GetExchangeRatesQueryValidator : AbstractValidator<GetExchangeRatesQuery>
{
    /// <summary>Upper limit protecting the provider from very large requests.</summary>
    public const int MaxRangeDays = 366;

    private const string CurrencyCodePattern = "^[A-Za-z]{3}$";

    public GetExchangeRatesQueryValidator(TimeProvider timeProvider)
    {
        RuleFor(q => q.ProviderKey)
            .NotEmpty()
            .WithMessage("Należy wybrać dostawcę kursów walut.");

        RuleFor(q => q.SourceCurrency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Waluta źródłowa jest wymagana.")
            .Matches(CurrencyCodePattern)
            .WithMessage("Kod waluty źródłowej musi składać się z trzech liter.");

        RuleFor(q => q.TargetCurrency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Waluta docelowa jest wymagana.")
            .Matches(CurrencyCodePattern)
            .WithMessage("Kod waluty docelowej musi składać się z trzech liter.")
            .Must((query, target) => !string.Equals(query.SourceCurrency, target, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Waluta docelowa musi być inna niż waluta źródłowa.");

        RuleFor(q => q).Custom((query, context) =>
        {
            var today = timeProvider.GetTodayInPoland();
            var (from, to) = query.ResolveDates(today);

            // Only the first broken date rule is reported - later ones would just repeat the same problem.
            if (to > today)
            {
                context.AddFailure(nameof(query.DateTo), "Data do nie może być datą z przyszłości.");
            }
            else if (from > to)
            {
                context.AddFailure(nameof(query.DateFrom), "Data od nie może być późniejsza niż data do.");
            }
            else if (to.DayNumber - from.DayNumber + 1 > MaxRangeDays)
            {
                context.AddFailure(nameof(query.DateFrom), $"Zakres dat nie może przekraczać {MaxRangeDays} dni.");
            }
        });
    }
}
