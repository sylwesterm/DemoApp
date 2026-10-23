using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace CurrencyRates.Api.ErrorHandling;

internal static class ErrorHandlingExtensions
{
    // Same output as the MVC JSON formatter: web defaults and Polish letters written as-is, not as \uXXXX escapes.
    private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Registers the ProblemDetails responses: the global exception handler and Polish responses
    /// for parameters that ASP.NET cannot bind (e.g. <c>dateFrom=abc</c>), which never reach the validators.
    /// </summary>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // PostConfigure runs after the MVC defaults set up by AddControllers, whatever the registration order.
        services.PostConfigure<MvcOptions>(options => UsePolishMessages(options.ModelBindingMessageProvider));
        services.PostConfigure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse);

        return services;
    }

    private static void UsePolishMessages(DefaultModelBindingMessageProvider messages)
    {
        messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"Wartość '{value}' jest nieprawidłowa dla pola {field}.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Wartość '{value}' jest nieprawidłowa.");
        messages.SetUnknownValueIsInvalidAccessor(field => $"Podana wartość jest nieprawidłowa dla pola {field}.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Podana wartość jest nieprawidłowa.");
        messages.SetValueIsInvalidAccessor(value => $"Wartość '{value}' jest nieprawidłowa.");
        messages.SetValueMustBeANumberAccessor(field => $"Pole {field} musi być liczbą.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "Pole musi być liczbą.");
        messages.SetValueMustNotBeNullAccessor(value => $"Wartość '{value}' jest nieprawidłowa.");
        messages.SetMissingBindRequiredValueAccessor(field => $"Brak wartości parametru {field}.");
        messages.SetMissingKeyOrValueAccessor(() => "Wartość jest wymagana.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "Treść żądania jest wymagana.");
    }

    private static JsonResult CreateInvalidModelStateResponse(ActionContext context)
    {
        var httpContext = context.HttpContext;

        // The ASP.NET factory adds "type" and "traceId" exactly as for the exception handler's responses.
        var problem = httpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(
                httpContext,
                context.ModelState,
                StatusCodes.Status400BadRequest,
                title: ProblemTitles.Validation);

        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(ErrorHandlingExtensions).FullName!);
        ErrorHandlingLog.RequestRejected(
            logger,
            httpContext.Request.Method,
            LogText.SingleLine(httpContext.Request.Path.Value ?? string.Empty),
            StatusCodes.Status400BadRequest,
            LogText.SingleLine(string.Join(" ", problem.Errors.SelectMany(error => error.Value))));

        // JsonResult rather than an ObjectResult: [Produces("application/json")] on the controllers
        // would overwrite the content type of an ObjectResult.
        return new JsonResult(problem, ProblemJsonOptions)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentType = "application/problem+json",
        };
    }
}
