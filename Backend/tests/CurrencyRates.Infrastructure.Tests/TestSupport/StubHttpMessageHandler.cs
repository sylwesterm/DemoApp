using System.Net;
using System.Text;

namespace CurrencyRates.Infrastructure.Tests.TestSupport;

/// <summary>
/// Replaces the network: answers each request with a response chosen by the test
/// (the call number starts at 1) and records the requested URIs.
/// </summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> respond) : HttpMessageHandler
{
    private int callCount;

    public List<Uri> RequestedUris { get; } = [];

    public int CallCount => callCount;

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Status(HttpStatusCode status) => new(status);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var call = Interlocked.Increment(ref callCount);
        lock (RequestedUris)
        {
            RequestedUris.Add(request.RequestUri!);
        }

        return Task.FromResult(respond(request, call));
    }
}
