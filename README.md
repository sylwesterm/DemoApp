# DemoApp

Aplikacja webowa do przeliczania kursów walut oraz przeglądania ich historii na podstawie danych z [API NBP](https://api.nbp.pl/).

## Technologie

- **Backend:** .NET 9 (ASP.NET Core Web API), logowanie z użyciem Serilog
- **Frontend:** React + TypeScript (Vite)

### Dlaczego React zamiast Angulara

W treści zadania jako frontend wskazany jest Angular. Ponieważ jest to aplikacja demonstracyjna, wybrałem React z Vite: konfiguracja jest lżejsza i szybsza, a projekt ma mniej kodu startowego. Backend udostępnia niezależne od frameworka REST API (`/providers`, `/currencies`, `/rates`), więc w przyszłości można dodać klienta w Angularze bez żadnych zmian po stronie serwera.

_Instrukcja uruchomienia projektu lokalnie zostanie dodana w trakcie rozwoju aplikacji._
