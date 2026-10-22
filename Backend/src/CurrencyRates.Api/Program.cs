using System.Globalization;
using CurrencyRates.Application;
using CurrencyRates.Infrastructure;
using Serilog;

// Console-only logger for the startup phase; replaced by the configured one once the host is built.
// A plain (non-reloadable) logger keeps several hosts in one process (integration tests) working.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    Log.Information("Uruchamianie aplikacji CurrencyRates.Api");

    var builder = WebApplication.CreateBuilder(args);

    // Levels, sinks and the log file path come from the "Serilog" section of appsettings.json.
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
        options.MessageTemplate =
            "Żądanie HTTP {RequestMethod} {RequestPath} zakończone kodem {StatusCode} w {Elapsed:0.0} ms");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Currency Rates API v1");
            options.DocumentTitle = "Currency Rates API";
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // HostAbortedException is thrown on purpose by tooling (e.g. dotnet ef, OpenAPI generation); it is not a crash.
    Log.Fatal(exception, "Aplikacja zakończyła działanie z powodu nieoczekiwanego błędu");
}
finally
{
    await Log.CloseAndFlushAsync();
}
