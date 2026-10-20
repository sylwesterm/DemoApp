namespace CurrencyRates.Infrastructure.Tests.TestSupport;

/// <summary>Responses in the exact shape returned by api.nbp.pl (captured 2026-10-20).</summary>
internal static class NbpJson
{
    public const string TableA = """
        [{"table":"A","no":"203/A/NBP/2026","effectiveDate":"2026-10-19","rates":[
          {"currency":"dolar amerykański","code":"USD","mid":3.8974},
          {"currency":"euro","code":"EUR","mid":4.3661}]}]
        """;

    public const string EurRates = """
        {"table":"A","currency":"euro","code":"EUR","rates":[
          {"no":"201/A/NBP/2026","effectiveDate":"2026-10-15","mid":4.3691},
          {"no":"202/A/NBP/2026","effectiveDate":"2026-10-16","mid":4.3661}]}
        """;

    /// <summary>The <c>rates</c> field is missing.</summary>
    public const string RatesWithoutRatesField = """{"table":"A","currency":"euro","code":"EUR"}""";
}
