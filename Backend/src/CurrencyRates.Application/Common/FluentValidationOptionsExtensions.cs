using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Application.Common;

public static class FluentValidationOptionsExtensions
{
    /// <summary>
    /// Validates the options with the registered <see cref="IValidator{T}"/>.
    /// </summary>
    public static OptionsBuilder<TOptions> ValidateWithFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<IValidateOptions<TOptions>>(provider =>
            new FluentValidationOptionsValidator<TOptions>(
                builder.Name,
                provider.GetRequiredService<IValidator<TOptions>>()));

        return builder;
    }
}
