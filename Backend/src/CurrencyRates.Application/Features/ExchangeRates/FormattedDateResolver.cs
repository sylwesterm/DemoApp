using AutoMapper;
using CurrencyRates.Application.Common;
using CurrencyRates.Application.Options;
using CurrencyRates.Domain.Models;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Application.Features.ExchangeRates;

/// <summary>
/// Formats a rate's date with the configured <see cref="DisplayOptions.DateFormat"/>.
/// </summary>
public sealed class FormattedDateResolver(IOptions<DisplayOptions> options)
    : IValueResolver<ExchangeRate, ExchangeRateDto, string>
{
    public string Resolve(ExchangeRate source, ExchangeRateDto destination, string destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Date.ToString(options.Value.DateFormat, DisplayCulture.Polish);
    }
}
