using CurrencyRates.Application.Features.Providers;
using CurrencyRates.Application.Features.Providers.GetProviders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.Controllers;

/// <summary>Exchange rate providers (data sources).</summary>
[ApiController]
[Route("api/providers")]
[Produces("application/json")]
public sealed class ProvidersController(ISender sender) : ControllerBase
{
    /// <summary>Lists all available exchange rate providers.</summary>
    /// <remarks>Feeds the "API" drop-down. A newly registered provider appears here automatically.</remarks>
    /// <response code="200">Providers, e.g. NBP.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProviderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProviderDto>>> GetProviders(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProvidersQuery(), cancellationToken));
}
