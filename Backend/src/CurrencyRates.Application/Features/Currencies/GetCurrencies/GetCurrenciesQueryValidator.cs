using FluentValidation;

namespace CurrencyRates.Application.Features.Currencies.GetCurrencies;

public sealed class GetCurrenciesQueryValidator : AbstractValidator<GetCurrenciesQuery>
{
    public GetCurrenciesQueryValidator()
    {
        RuleFor(q => q.ProviderKey)
            .NotEmpty()
            .WithMessage("Należy wybrać dostawcę kursów walut.");
    }
}
