using System.Text.Json;
using CurrencyRates.Application.Exceptions;
using CurrencyRates.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Api.ErrorHandling;

/// <summary>
/// Translates exceptions escaping the controllers into ProblemDetails responses with Polish titles
/// and writes them to the log: rejected requests as warnings (with the stack trace only for domain rule
/// violations), provider failures and bugs as errors with the full stack trace.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // A request aborted by the client never gets here: ExceptionHandlerMiddleware answers it with 499 itself.
        var method = httpContext.Request.Method;
        var path = LogText.SingleLine(httpContext.Request.Path.Value ?? string.Empty);

        var problem = CreateProblemDetails(exception);
        var statusCode = problem.Status!.Value;

        switch (statusCode)
        {
            case < StatusCodes.Status500InternalServerError when exception is DomainException:
                ErrorHandlingLog.DomainRuleViolated(logger, exception, method, path, statusCode, LogText.SingleLine(exception.Message));
                break;
            // Validation and 404: the reason says it all, a stack trace would only repeat the MediatR pipeline.
            case < StatusCodes.Status500InternalServerError:
                ErrorHandlingLog.RequestRejected(logger, method, path, statusCode, LogText.SingleLine(DescribeReason(problem)));
                break;
            case StatusCodes.Status500InternalServerError:
                ErrorHandlingLog.UnhandledException(logger, exception, method, path);
                break;
            default:
                ErrorHandlingLog.ProviderFailed(logger, exception, method, path, statusCode, LogText.SingleLine(exception.Message));
                break;
        }

        httpContext.Response.StatusCode = statusCode;

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });

        // TryWriteAsync declines when the Accept header rules out JSON (e.g. "text/plain"). Returning false would
        // let the middleware write its own English response and log the exception again, so write JSON anyway.
        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problem,
                problem.GetType(),
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
        }

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(ToErrors(validation))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ProblemTitles.Validation,
        },
        DomainException => Problem(StatusCodes.Status400BadRequest, ProblemTitles.InvalidData, exception.Message),
        NotFoundException => Problem(StatusCodes.Status404NotFound, ProblemTitles.NotFound, exception.Message),
        // The more specific timeout must come before its base class.
        ExternalServiceTimeoutException => Problem(StatusCodes.Status504GatewayTimeout, ProblemTitles.ProviderTimeout, exception.Message),
        ExternalServiceException => Problem(StatusCodes.Status502BadGateway, ProblemTitles.ProviderError, exception.Message),
        _ => Problem(StatusCodes.Status500InternalServerError, ProblemTitles.Unexpected, ProblemTitles.UnexpectedDetail),
    };

    private static ProblemDetails Problem(int statusCode, string title, string detail) =>
        new() { Status = statusCode, Title = title, Detail = detail };

    /// <summary>
    /// Groups validation messages by property, camel-cased like the query parameters and the JSON payload
    /// (e.g. <c>sourceCurrency</c>, <c>dateFrom</c>), so the frontend can show each message next to its field.
    /// </summary>
    private static Dictionary<string, string[]> ToErrors(ValidationException exception) =>
        exception.Errors
            .GroupBy(failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

    /// <summary>
    /// Text for the log. Validation errors are listed one by one, because the message of a <see cref="ValidationException"/>
    /// starts with an English prefix generated by FluentValidation.
    /// </summary>
    private static string DescribeReason(ProblemDetails problem) =>
        problem is ValidationProblemDetails validation
            ? string.Join(" ", validation.Errors.SelectMany(error => error.Value))
            : problem.Detail ?? problem.Title ?? string.Empty;
}
