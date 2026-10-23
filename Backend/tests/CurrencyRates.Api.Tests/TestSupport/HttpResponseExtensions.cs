using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.Tests.TestSupport;

internal static class HttpResponseExtensions
{
    public static async Task<ProblemDetails> ReadProblemAsync(this HttpResponseMessage response)
    {
        (response.Content.Headers.ContentType?.MediaType).ShouldBe("application/problem+json");
        return (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
    }

    public static async Task<ValidationProblemDetails> ReadValidationProblemAsync(this HttpResponseMessage response)
    {
        (response.Content.Headers.ContentType?.MediaType).ShouldBe("application/problem+json");
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
    }
}
