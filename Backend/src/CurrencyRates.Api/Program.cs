using System.Globalization;
using CurrencyRates.Api.ErrorHandling;
using CurrencyRates.Application;
using CurrencyRates.Infrastructure;
using Serilog;
using Serilog.Events;

const string StartupFailedMessage = "Aplikacja zakończyła działanie z powodu nieoczekiwanego błędu";

// Console-only logger for the startup phase, before the configuration is read.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

IConfiguration? configuration = null;
WebApplication? app = null;

try
{
    Log.Information("Uruchamianie aplikacji CurrencyRates.Api");

    var builder = WebApplication.CreateBuilder(args);
    configuration = builder.Configuration;

    // Levels, sinks and the log file path come from the "Serilog" section of appsettings.json.
    // preserveStaticLogger: the host keeps its own logger instead of routing through the static Log.Logger,
    // so several hosts in one process (parallel integration tests) do not write into each other's sinks.
    builder.Services.AddSerilog(
        (services, logger) => logger
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services),
        preserveStaticLogger: true);

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddErrorHandling();
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    app = builder.Build();

    // Request logging wraps the exception handler, so it logs the final status code (e.g. 404), not the exception.
    // Failures are logged once, as errors with the exception, by GlobalExceptionHandler, so the summary line stays
    // Information; Error only for an exception that got past the handler.
    app.UseSerilogRequestLogging(options =>
    {
        options.Logger = app.Services.GetRequiredService<Serilog.ILogger>();
        options.MessageTemplate =
            "Żądanie HTTP {RequestMethod} {RequestPath} zakończone kodem {StatusCode} w {Elapsed:0.0} ms";
        options.GetLevel = (_, _, exception) =>
            exception is null ? LogEventLevel.Information : LogEventLevel.Error;
    });
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Currency Rates API v1");
            options.DocumentTitle = "Currency Rates API";
        });
    }

    // In Development the frontend calls the API through the Vite dev-server proxy over plain HTTP. A redirect to
    // HTTPS would send the browser to another origin (https://localhost:7245), where the request fails on CORS.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // HostAbortedException is thrown on purpose by tooling (e.g. dotnet ef, OpenAPI generation); it is not a crash.
    await LogStartupFailureAsync(exception);
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// The host's logger is gone together with the host (RunAsync disposes it), so the crash is written by a short-lived
// logger built from the same configuration - it reaches the log file too. If even that is impossible, the console
// logger is used.
async Task LogStartupFailureAsync(Exception exception)
{
    if (app is not null)
    {
        // Releases the log file if the host still holds it, e.g. when the failure happened before RunAsync.
        await app.DisposeAsync();
    }

    if (configuration is not null)
    {
        try
        {
            await using var crashLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            crashLogger.ForContext(Serilog.Core.Constants.SourceContextPropertyName, "CurrencyRates.Api")
                .Fatal(exception, StartupFailedMessage);
            return;
        }
        catch (Exception loggerException) when (loggerException is not OutOfMemoryException)
        {
            Log.Error(loggerException, "Nie udało się utworzyć loggera z konfiguracji");
        }
    }

    Log.Fatal(exception, StartupFailedMessage);
}
