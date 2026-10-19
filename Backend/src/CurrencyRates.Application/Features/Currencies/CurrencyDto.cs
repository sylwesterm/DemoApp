namespace CurrencyRates.Application.Features.Currencies;

public sealed record CurrencyDto
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    /// <summary>Drop-down label in the <c>code - name</c> format, e.g. <c>CAD - dolar kanadyjski</c>.</summary>
    public required string Label { get; init; }
}
