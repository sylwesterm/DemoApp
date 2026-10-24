# Build: restore first (cached until a project or package file changes), then publish the API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/CurrencyRates.Domain/CurrencyRates.Domain.csproj src/CurrencyRates.Domain/
COPY src/CurrencyRates.Application/CurrencyRates.Application.csproj src/CurrencyRates.Application/
COPY src/CurrencyRates.Infrastructure/CurrencyRates.Infrastructure.csproj src/CurrencyRates.Infrastructure/
COPY src/CurrencyRates.Api/CurrencyRates.Api.csproj src/CurrencyRates.Api/
RUN dotnet restore src/CurrencyRates.Api/CurrencyRates.Api.csproj

COPY src/ src/
RUN dotnet publish src/CurrencyRates.Api/CurrencyRates.Api.csproj \
    --configuration Release --no-restore --output /app /p:UseAppHost=false

# Runtime: the smaller ASP.NET image, running as the image's non-root user.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Log files (Serilog: logs/currency-rates-<date>.log) - created here so the non-root user can write to it,
# also when a named volume is mounted over it.
RUN mkdir logs && chown "$APP_UID" logs
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CurrencyRates.Api.dll"]
