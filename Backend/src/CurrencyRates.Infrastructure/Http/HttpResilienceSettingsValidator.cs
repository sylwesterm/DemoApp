using FluentValidation;

namespace CurrencyRates.Infrastructure.Http;

public sealed class HttpResilienceSettingsValidator : AbstractValidator<HttpResilienceSettings>
{
    public HttpResilienceSettingsValidator()
    {
        RuleFor(s => s.MaxRetryAttempts)
            .InclusiveBetween(1, 10)
            .WithMessage("Liczba ponowień (MaxRetryAttempts) musi mieścić się w zakresie 1-10.");

        RuleFor(s => s.RetryBaseDelay)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Opóźnienie ponowienia (RetryBaseDelay) musi być większe od zera.");

        RuleFor(s => s.AttemptTimeout)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Limit czasu próby (AttemptTimeout) musi być większy od zera.");

        RuleFor(s => s.TotalRequestTimeout)
            .GreaterThanOrEqualTo(s => s.AttemptTimeout)
            .WithMessage("Całkowity limit czasu (TotalRequestTimeout) nie może być krótszy niż limit czasu próby (AttemptTimeout).");
    }
}
