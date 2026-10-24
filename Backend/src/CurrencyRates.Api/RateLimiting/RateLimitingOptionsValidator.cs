using FluentValidation;

namespace CurrencyRates.Api.RateLimiting;

public sealed class RateLimitingOptionsValidator : AbstractValidator<RateLimitingOptions>
{
    public RateLimitingOptionsValidator()
    {
        RuleFor(o => o.PermitLimit)
            .InclusiveBetween(1, 10_000)
            .WithMessage($"Limit zapytań ({RateLimitingOptions.SectionName}:{nameof(RateLimitingOptions.PermitLimit)}) " +
                         "musi wynosić od 1 do 10000.");

        RuleFor(o => o.Window)
            .InclusiveBetween(TimeSpan.FromSeconds(1), TimeSpan.FromHours(1))
            .WithMessage($"Okno limitu zapytań ({RateLimitingOptions.SectionName}:{nameof(RateLimitingOptions.Window)}) " +
                         "musi trwać od 1 sekundy do 1 godziny.");
    }
}
