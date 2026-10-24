using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.ErrorHandling;

internal static class ProblemDetailsWriter
{
    /// <summary>
    /// Writes <paramref name="problem"/> as <c>application/problem+json</c>. <see cref="IProblemDetailsService"/>
    /// declines when the Accept header rules out JSON (e.g. "text/plain"); the response is written anyway, so the
    /// client always gets the Polish message instead of a framework default.
    /// </summary>
    public static async Task WriteAsync(HttpContext httpContext, ProblemDetails problem, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        var written = await httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem });

        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problem,
                problem.GetType(),
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
        }
    }
}
