using System.Globalization;
using System.Threading.RateLimiting;
using CurrencyRates.Api.ErrorHandling;
using CurrencyRates.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Api.RateLimiting;

internal static class RateLimitingExtensions
{
    /// <summary>Policy applied to the API controllers (Swagger and the OpenAPI document are not limited).</summary>
    public const string ApiPolicy = "api";

    /// <summary>
    /// A fixed window of <see cref="RateLimitingOptions.PermitLimit"/> requests per client IP address.
    /// Behind a reverse proxy the client address comes from X-Forwarded-For only when forwarded headers are
    /// enabled (ASPNETCORE_FORWARDEDHEADERS_ENABLED, set in compose.yaml) - otherwise all clients would share
    /// the proxy's address and therefore one limit.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidator<RateLimitingOptions>, RateLimitingOptionsValidator>();
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateWithFluentValidation()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = OnRejectedAsync;
            options.AddPolicy(ApiPolicy, httpContext =>
            {
                var settings = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter(
                    ClientAddress(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = settings.Window,
                        // Excess requests are rejected at once rather than queued - the client is told when to retry.
                        QueueLimit = 0,
                    });
            });
        });

        return services;
    }

    private static string ClientAddress(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "nieznany";

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var settings = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

        var detail = string.Create(
            CultureInfo.InvariantCulture,
            $"Dozwolonych jest {settings.PermitLimit} zapytań w ciągu {settings.Window.TotalSeconds:0} s.");
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
            httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
            detail += string.Create(CultureInfo.InvariantCulture, $" Spróbuj ponownie za {seconds} s.");
        }

        var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RateLimitingExtensions).FullName!);
        RateLimitingLog.LimitExceeded(
            logger,
            ClientAddress(httpContext),
            httpContext.Request.Method,
            LogText.SingleLine(httpContext.Request.Path.Value ?? string.Empty));

        await ProblemDetailsWriter.WriteAsync(
            httpContext,
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = ProblemTitles.TooManyRequests,
                Detail = detail,
            },
            cancellationToken);
    }
}
