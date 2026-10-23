# DemoApp

Aplikacja webowa do przeliczania kursów walut oraz przeglądania ich historii na podstawie danych z [API NBP](https://api.nbp.pl/).

## Technologie

- **Backend:** .NET 10 (ASP.NET Core Web API), Clean Architecture, CQRS (MediatR), AutoMapper, FluentValidation, Serilog
- **Frontend:** React + TypeScript (Vite)

### Dlaczego React zamiast Angulara

W treści zadania jako frontend wskazany jest Angular. Ponieważ jest to aplikacja demonstracyjna, wybrałem React z Vite: konfiguracja jest lżejsza i szybsza, a projekt ma mniej kodu startowego. Backend udostępnia niezależne od frameworka REST API (`/api/providers`, `/api/providers/{dostawca}/currencies`, `/api/providers/{dostawca}/rates`), więc w przyszłości można dodać klienta w Angularze bez żadnych zmian po stronie serwera.

## Wymagania

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (wersja 10.0.100 lub nowsza, patrz `Backend/global.json`)
- opcjonalnie Visual Studio 2026 lub inne IDE obsługujące pliki `.slnx`
- dostęp do internetu (aplikacja pobiera kursy z `https://api.nbp.pl`)

## Uruchomienie backendu

Z wiersza poleceń:

```bash
cd Backend
dotnet run --project src/CurrencyRates.Api
```

W Visual Studio: otwórz `Backend/CurrencyRates.slnx`, ustaw `CurrencyRates.Api` jako projekt startowy i uruchom (F5).

Po starcie API jest dostępne pod adresem `http://localhost:5178`, a dokumentacja Swagger pod adresem **http://localhost:5178/swagger** (tylko w środowisku `Development`). Gotowe przykładowe zapytania znajdują się w pliku `Backend/src/CurrencyRates.Api/CurrencyRates.Api.http`, który można uruchomić bezpośrednio w Visual Studio lub VS Code.

### Endpointy

| Metoda i adres | Opis |
|---|---|
| `GET /api/providers` | Lista dostawców kursów (lista rozwijana „API”). |
| `GET /api/providers/{dostawca}/currencies` | Waluty dostawcy w formacie `kod - nazwa`, posortowane zgodnie z konfiguracją. |
| `GET /api/providers/{dostawca}/rates?sourceCurrency=EUR&targetCurrency=USD&dateFrom=2026-10-01&dateTo=2026-10-16` | Kursy waluty źródłowej w walucie docelowej: minimum, maksimum, średnia i tabela kursów z datą w skonfigurowanym formacie. Pominięta data oznacza dzisiejszy dzień (czas polski). Zakres może obejmować maksymalnie 366 dni. |

### Kody odpowiedzi

| Kod | Kiedy |
|---|---|
| `200 OK` | Dane zwrócone poprawnie. |
| `204 No Content` | Brak kursów w wybranym okresie (np. weekend, święto lub dzisiejsza tabela NBP nie została jeszcze opublikowana – NBP publikuje ją ok. 11:45). |
| `400 Bad Request` | Nieprawidłowe parametry, np. brak waluty, ta sama waluta źródłowa i docelowa, data z przyszłości, błędny format daty. Odpowiedź zawiera komunikaty po polsku przypisane do pól. |
| `404 Not Found` | Nieznany dostawca lub waluta nieobsługiwana przez dostawcę. |
| `502 Bad Gateway` | Dostawca zwrócił błąd lub nieprawidłowe dane albo jest nieosiągalny. |
| `504 Gateway Timeout` | Dostawca nie odpowiedział w wymaganym czasie. |
| `500 Internal Server Error` | Nieoczekiwany błąd. Klient dostaje ogólny komunikat, a szczegóły trafiają do logu. |

Błędy są zwracane w standardowym formacie [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`) z polskim tytułem i opisem. Pole `traceId` pozwala odnaleźć dany błąd w pliku logu.

## Konfiguracja

Cała konfiguracja znajduje się w pliku `Backend/src/CurrencyRates.Api/appsettings.json`. Jest sprawdzana przy starcie aplikacji: nieprawidłowa wartość (np. format daty bez roku) zatrzymuje start z czytelnym komunikatem w logu.

| Klucz | Znaczenie | Domyślnie |
|---|---|---|
| `Display:DateFormat` | Format daty w tabeli kursów (np. `dd.MM.yyyy`). | `yyyy-MM-dd` |
| `Display:CurrencySortOrder` | Sortowanie walut: `CodeAscending`, `CodeDescending`, `NameAscending`, `NameDescending`. | `CodeAscending` |
| `CurrencyProviders:Nbp:BaseUrl` | Adres API NBP. | `https://api.nbp.pl/api/` |
| `CurrencyProviders:Nbp:MaxDaysPerRequest` | Maksymalna liczba dni w jednym zapytaniu do NBP (dłuższe zakresy są dzielone). | `367` |
| `CurrencyProviders:Nbp:CurrencyListCacheDuration` | Jak długo lista walut jest przechowywana w pamięci podręcznej. | `01:00:00` |
| `CurrencyProviders:Nbp:Resilience:*` | Ponowienia i limity czasu (patrz niżej). | 3 ponowienia, 0,5 s, 10 s, 30 s |
| `Serilog:MinimumLevel:Default` | Poziom logowania (`Debug`, `Information`, `Warning`, `Error`). | `Information` (`Debug` w `Development`) |
| `Serilog:WriteTo:1:Args:path` | Ścieżka pliku logu. | `logs/currency-rates-.log` |

Każdą wartość można nadpisać zmienną środowiskową, zastępując `:` podwójnym podkreśleniem, np.:

```bash
# bash
Display__DateFormat=dd.MM.yyyy dotnet run --project src/CurrencyRates.Api
```

```powershell
# PowerShell
$env:Display__DateFormat = "dd.MM.yyyy"; dotnet run --project src/CurrencyRates.Api
```

### Klucz licencji AutoMapper (opcjonalny)

AutoMapper od wersji 15 wymaga klucza licencyjnego (dostępna jest bezpłatna licencja Community). Bez klucza aplikacja działa normalnie, a przy starcie zapisuje jedynie ostrzeżenie w logu. Klucza nie należy umieszczać w repozytorium – można go zapisać w sekretach użytkownika:

```bash
cd Backend
dotnet user-secrets set "AutoMapper:LicenseKey" "<klucz>" --project src/CurrencyRates.Api
```

albo przekazać zmienną środowiskową `AutoMapper__LicenseKey`.

## Logi

Logi są zapisywane na konsoli oraz do pliku `Backend/src/CurrencyRates.Api/logs/currency-rates-RRRRMMDD.log`. Codziennie tworzony jest nowy plik, a przechowywanych jest 14 ostatnich. Ścieżka jest względna wobec katalogu roboczego, którym przy uruchomieniu z Visual Studio lub przez `dotnet run --project` jest katalog projektu.

- błędy dostawcy (502, 504) i błędy nieoczekiwane (500) są logowane na poziomie `Error` z pełnym stosem wywołań, łącznie z wyjątkami wewnętrznymi;
- odrzucone żądania (400, 404) są logowane jako `Warning` w jednej linii z przyczyną; dla naruszenia reguły domeny dołączany jest stos wywołań;
- każde ponowienie wywołania NBP jest logowane jako `Warning` z przyczyną i opóźnieniem;
- błąd podczas startu aplikacji jest zapisywany jako `Fatal`, a proces kończy się kodem 1.

Wszystkie komunikaty aplikacji są po polsku. Nieliczne komunikaty samego frameworka (np. „Now listening on…”) pozostają po angielsku.

## Odporność na błędy API

Wywołania API NBP są zabezpieczone (biblioteka `Microsoft.Extensions.Http.Resilience`):

- **ponowienia z wykładniczym opóźnieniem i losowym rozrzutem (jitter)** – dla błędów sieci, kodów 408, 429 i 5xx;
- **limit czasu pojedynczej próby** oraz **całkowity limit czasu** zapytania;
- **circuit breaker** – przy serii błędów kolejne wywołania są na chwilę wstrzymywane, zamiast obciążać niedziałający serwis.

Parametry ustawia się w sekcji `CurrencyProviders:Nbp:Resilience` (`MaxRetryAttempts`, `RetryBaseDelay`, `AttemptTimeout`, `TotalRequestTimeout`). Lista walut jest dodatkowo przechowywana w pamięci podręcznej, a zakresy dłuższe niż limit NBP są dzielone na kilka równoległych zapytań.

## Testy

```bash
cd Backend
dotnet test CurrencyRates.slnx
```

| Projekt | Zakres |
|---|---|
| `CurrencyRates.Domain.Tests` | Reguły modeli domenowych (waluta, zakres dat, kurs, statystyki). |
| `CurrencyRates.Application.Tests` | Obsługa zapytań, walidacja, kursy krzyżowe, statystyki, sortowanie, mapowanie. |
| `CurrencyRates.Infrastructure.Tests` | Klient API NBP (na atrapie HTTP), ponowienia, dzielenie zakresów, konfiguracja. |
| `CurrencyRates.Api.Tests` | Testy integracyjne endpointów i obsługi błędów (`WebApplicationFactory`, bez dostępu do sieci) oraz testy architektury pilnujące zależności między warstwami i konwencji. |

## Architektura

Backend jest podzielony zgodnie z Clean Architecture – zależności wskazują do środka, co sprawdzają testy architektury:

```
Backend/src
├── CurrencyRates.Domain          modele i reguły biznesowe, bez zależności
├── CurrencyRates.Application     przypadki użycia (zapytania CQRS), walidacja, kursy krzyżowe, interfejsy dostawców
├── CurrencyRates.Infrastructure  implementacje dostawców (NBP), klient HTTP, ponowienia, pamięć podręczna
└── CurrencyRates.Api             kontrolery REST, obsługa błędów, logowanie, konfiguracja DI
```

- Kontrolery tylko wysyłają zapytanie przez MediatR (`ISender`); logika jest w handlerach warstwy Application.
- Walidacja (FluentValidation) działa jako element potoku MediatR, więc żaden handler nie dostanie nieprawidłowego zapytania.
- Wszystkie kursy dostawca podaje w swojej walucie bazowej (dla NBP – PLN), a kurs dowolnej pary walut wylicza `CrossRateCalculator`.

### Dodanie nowego dostawcy kursów

1. W `CurrencyRates.Infrastructure/Providers/<Nazwa>/` utwórz klasę implementującą `ICurrencyRateProvider` (klucz, nazwa, waluta bazowa, lista walut, kursy).
2. Zarejestruj ją w `Infrastructure/DependencyInjection.cs` razem z klientem HTTP i `AddConfiguredResilience(...)`.
3. Dodaj jej sekcję w `appsettings.json` (adres API, ustawienia ponowień).

Nowy dostawca automatycznie pojawi się w `GET /api/providers`, a więc i na liście w aplikacji. Kontrolery, handlery ani frontend nie wymagają zmian.

## Frontend

_Instrukcja uruchomienia frontendu zostanie dodana wraz z jego implementacją._
