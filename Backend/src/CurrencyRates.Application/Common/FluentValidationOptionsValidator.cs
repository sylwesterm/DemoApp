using FluentValidation;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Application.Common;

/// <summary>
/// Bridges FluentValidation validators into the options validation pipeline,
/// so configuration is validated with the same library as requests.
/// </summary>
public sealed class FluentValidationOptionsValidator<TOptions>(string? optionsName, IValidator<TOptions> validator)
    : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (optionsName is not null && optionsName != name)
        {
            return ValidateOptionsResult.Skip;
        }

        var result = validator.Validate(options);

        return result.IsValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(result.Errors.Select(error => error.ErrorMessage));
    }
}
