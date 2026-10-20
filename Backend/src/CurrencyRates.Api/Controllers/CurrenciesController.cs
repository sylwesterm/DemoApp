using CurrencyRates.Application.Features.Currencies;
using CurrencyRates.Application.Features.Currencies.GetCurrencies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.Controllers;

/// <summary>Currencies offered by a provider.</summary>
[ApiController]
[Route("api/providers/{provider}/currencies")]
[Produces("application/json")]
public sealed class CurrenciesController(ISender sender) : ControllerBase
{
    /// <summary>Lists currencies offered by the provider.</summary>
    /// <remarks>
    /// Sorted according to <c>Display:CurrencySortOrder</c> in the configuration.
    /// Each item has a <c>label</c> in the "code - name" format, e.g. "CAD - dolar kanadyjski".
    /// </remarks>
    /// <param name="provider">Provider key, e.g. <c>NBP</c> (case-insensitive).</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="200">Sorted currencies.</response>
    /// <response code="404">Unknown provider.</response>
    /// <response code="502">The provider returned an error or invalid data.</response>
    /// <response code="504">The provider did not respond in time.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CurrencyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<CurrencyDto>>> GetCurrencies(
        string provider,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetCurrenciesQuery(provider), cancellationToken));
}
