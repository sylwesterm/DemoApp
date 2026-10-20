using CurrencyRates.Infrastructure.Http;
using FluentValidation;

namespace CurrencyRates.Infrastructure.Providers.Nbp;

public sealed class NbpOptionsValidator : AbstractValidator<NbpOptions>
{
    /// <summary>Limit enforced by the NBP API (a longer range returns HTTP 400).</summary>
    public const int NbpMaxDaysPerRequest = 367;

    public NbpOptionsValidator()
    {
        RuleFor(o => o.DisplayName)
            .NotEmpty()
            .WithMessage($"Nazwa dostawcy ({NbpOptions.SectionName}:DisplayName) nie może być pusta.");

        RuleFor(o => o.BaseUrl)
            .Must(url => url is { IsAbsoluteUri: true } && (url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeHttp))
            .WithMessage($"Adres API NBP ({NbpOptions.SectionName}:BaseUrl) musi być bezwzględnym adresem http(s).")
            .Must(url => url is null || url.AbsoluteUri.EndsWith('/'))
            .WithMessage($"Adres API NBP ({NbpOptions.SectionName}:BaseUrl) musi kończyć się znakiem '/'.");

        RuleFor(o => o.MaxDaysPerRequest)
            .InclusiveBetween(1, NbpMaxDaysPerRequest)
            .WithMessage($"Maksymalny zakres zapytania (MaxDaysPerRequest) musi mieścić się w zakresie 1-{NbpMaxDaysPerRequest} dni.");

        RuleFor(o => o.CurrencyListCacheDuration)
            .GreaterThanOrEqualTo(TimeSpan.Zero)
            .WithMessage("Czas buforowania listy walut (CurrencyListCacheDuration) nie może być ujemny.");

        RuleFor(o => o.Resilience)
            .NotNull()
            .SetValidator(new HttpResilienceSettingsValidator());
    }
}
