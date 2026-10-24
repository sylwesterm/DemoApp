namespace CurrencyRates.Api.ErrorHandling;

/// <summary>Polish titles of the ProblemDetails responses; the detail carries the specific message.</summary>
internal static class ProblemTitles
{
    public const string Validation = "Nieprawidłowe parametry żądania.";
    public const string InvalidData = "Nieprawidłowe dane.";
    public const string NotFound = "Nie znaleziono zasobu.";
    public const string ProviderError = "Błąd dostawcy kursów walut.";
    public const string ProviderTimeout = "Dostawca kursów walut nie odpowiedział w wymaganym czasie.";
    public const string Unexpected = "Wystąpił nieoczekiwany błąd.";
    public const string TooManyRequests = "Zbyt wiele zapytań.";

    /// <summary>Detail of a 500 response; the real cause is only written to the log, never sent to the client.</summary>
    public const string UnexpectedDetail = "Spróbuj ponownie później. Szczegóły błędu zapisano w dzienniku aplikacji.";
}
