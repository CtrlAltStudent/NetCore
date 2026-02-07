# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

(No changes yet.)

## [1.0.1] - 2026-02-07

### Fixed

- **MAUI (Windows):** Błąd inicjalizacji przy starcie – kontener DI był niedostępny (`Handler?.MauiContext?.Services` null). Aplikacja korzysta teraz z `App.Services` (wstrzyknięty `IServiceProvider` w `App`).
- **MAUI:** Po udanym logowaniu brak przejścia do Dashboardu – `LoginPage` używa `App.Services` do pobrania `AppShell`.
- **MAUI:** Logowanie na Windows z HTTPS – `HttpClient` akceptuje certyfikat deweloperski localhost (tylko w DEBUG).
- **MAUI:** Komunikat „Nieprawidłowy email lub hasło” przy błędzie połączenia z API – rozróżnienie: osobny komunikat przy braku połączenia i przy 401.
- **MAUI:** Sesja wygasła bez możliwości ponownego logowania – weryfikacja tokenu przy starcie (401 → wylogowanie, ekran logowania) oraz przycisk **Wyloguj** w Shell.

### Added

- **API:** Endpoint `POST /api/v1/seed/test-data` – wypełnienie organizacji danymi testowymi (okres, kanały, działy, pracownicy, przychody, koszty). Parametr `?force=true` zastępuje istniejące dane.
- **MAUI:** Przycisk „Załaduj dane testowe” na Dashboardzie oraz czytelne komunikaty błędów (404, 401, błąd połączenia).
- **Dokumentacja:** `docs/INSTRUKCJA-URUCHOMIENIA.md` – szczegółowa instrukcja uruchomienia; sekcja „Typowe problemy” (blokada plików, 404 seed, błąd inicjalizacji).

## [0.9.0-beta.1] - (Beta 1.0)

### Added

- **Backend (ASP.NET Core):**
  - REST API v1 (`/api/v1/`) with JWT authentication (register, login).
  - CRUD: sales channels, departments, employees, periods, revenues, costs (with cost assignments), bonus rules.
  - Reports: margin, operating result, by-channel, by-department.
  - Bonuses: calculate endpoint.
  - Swagger/OpenAPI documentation.
- **Financial engine (C#):**
  - Margin calculator (revenue, costs, allocation by weight).
  - Bonus calculator (e.g. % of department profit).
- **Infrastructure:** EF Core, PostgreSQL, initial migration.
- **.NET MAUI client (Windows, Android, iOS):**
  - Login, dashboard (operating result), channels list.
  - Reports (margin by period, by channel, by department), bonuses (calculate by period).
  - Config: channels, departments, periods.
- **CI:** GitHub Actions (build and test backend).
- Solution skeleton: Api, Domain, FinancialEngine, Infrastructure, Maui; README, CHANGELOG.

## [0.9.0] - (Beta 1.0 – planned)

(Same as 0.9.0-beta.1 when released as beta.)

## [1.0.0] - (planned)

### Added

- (Placeholder for first stable release)

[Unreleased]: https://github.com/CtrlAltStudent/NetCore/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/CtrlAltStudent/NetCore/compare/v0.9.0-beta.1...v1.0.1
[0.9.0-beta.1]: https://github.com/CtrlAltStudent/NetCore/releases/tag/v0.9.0-beta.1
[0.9.0]: https://github.com/CtrlAltStudent/NetCore/compare/v0.9.0-beta.1...v0.9.0
[1.0.0]: https://github.com/CtrlAltStudent/NetCore/releases/tag/v1.0.0
