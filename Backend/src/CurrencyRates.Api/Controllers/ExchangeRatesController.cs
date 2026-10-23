using CurrencyRates.Application.Features.ExchangeRates;
using CurrencyRates.Application.Features.ExchangeRates.GetExchangeRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.Controllers;

/// <summary>Exchange rate history between two currencies.</summary>
[ApiController]
[Route("api/providers/{provider}/rates")]
[Produces("application/json")]
public sealed class ExchangeRatesController(ISender sender) : ControllerBase
{
    /// <summary>Converts rates of the source currency into the target currency for a period.</summary>
    /// <remarks>
    /// Returns the minimum, maximum and average rate and one row per day on which both currencies
    /// have a published rate. A missing date means today (in Poland). The range is limited to 366 days.
    /// </remarks>
    /// <param name="provider">Provider key, e.g. <c>NBP</c> (case-insensitive).</param>
    /// <param name="sourceCurrency">Source currency code, e.g. <c>EUR</c>.</param>
    /// <param name="targetCurrency">Target currency code, e.g. <c>PLN</c>.</param>
    /// <param name="dateFrom">Start of the period (<c>yyyy-MM-dd</c>); today when omitted.</param>
    /// <param name="dateTo">End of the period (<c>yyyy-MM-dd</c>); today when omitted.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="200">Rates and statistics.</response>
    /// <response code="204">The provider published no rates in the period (e.g. a weekend or holiday).</response>
    /// <response code="400">Invalid parameters (missing or equal currencies, wrong dates).</response>
    /// <response code="404">Unknown provider or currency not offered by the provider.</response>
    /// <response code="502">The provider returned an error or invalid data.</response>
    /// <response code="504">The provider did not respond in time.</response>
    [HttpGet]
    [ProducesResponseType<ExchangeRateHistoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<ExchangeRateHistoryDto>> GetRates(
        string provider,
        [FromQuery] string? sourceCurrency,
        [FromQuery] string? targetCurrency,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        CancellationToken cancellationToken)
    {
        // Missing currencies are passed on as empty, so the validator reports them with its Polish messages.
        var query = new GetExchangeRatesQuery(provider, sourceCurrency ?? string.Empty, targetCurrency ?? string.Empty, dateFrom, dateTo);
        var result = await sender.Send(query, cancellationToken);

        return result is null ? NoContent() : Ok(result);
    }
}
