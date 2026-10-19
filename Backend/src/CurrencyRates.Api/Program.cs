var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

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
