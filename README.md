# NetCore

NetCore to aplikacja analityczna dla biznesu e-commerce, pokazująca **realną rentowność** (zysk netto operacyjny), a nie sam obrót. Umożliwia modelowanie kosztów stałych i zmiennych, przypisywanie ich do kanałów sprzedaży, działów i pracowników oraz śledzenie marży i wyniku finansowego. Zawiera moduł premiowania powiązany z wynikiem finansowym firmy.

## Stack

- **Backend:** ASP.NET Core (REST API), C#
- **Silnik finansowy:** biblioteka C# (marża, alokacja kosztów, premie)
- **Baza danych:** PostgreSQL
- **Klient:** .NET MAUI (Windows, Android, iOS)

## Struktura repozytorium

```
NetCore/
├── src/
│   ├── NetCore.Api/           # ASP.NET Core Web API
│   ├── NetCore.Domain/        # Encje i reguły domenowe
│   ├── NetCore.FinancialEngine/ # Silnik: marża, alokacja, premie
│   ├── NetCore.Infrastructure/  # EF Core, PostgreSQL
│   └── NetCore.Maui/          # Aplikacja .NET MAUI
├── tests/
├── docs/
└── .github/workflows/
```

## Wymagania

- .NET 10 SDK
- PostgreSQL (dla API)
- Dla MAUI: workload `dotnet workload install maui`

## Uruchomienie

### API

1. Utwórz bazę PostgreSQL i ustaw `ConnectionStrings:DefaultConnection` w `src/NetCore.Api/appsettings.json`.
2. Uruchom: `cd src/NetCore.Api && dotnet run`.
3. Swagger: https://localhost:5xxx/swagger (port w `Properties/launchSettings.json`).

### MAUI

1. W `src/NetCore.Maui/MauiProgram.cs` ustaw `baseUrl` na adres API (np. `https://localhost:5001`).
2. Windows: `cd src/NetCore.Maui && dotnet build -t:Run -f net10.0-windows10.0.19041.0`.

Szczegóły: [docs/user-guide.md](docs/user-guide.md).

## Wersjonowanie

Zgodnie z SemVer. Wersje beta: np. `0.9.0-beta.1`. Pełna wersja: `1.0.0`. Szczegóły w [CHANGELOG.md](CHANGELOG.md).

## Licencja

Projekt prywatny / do ustalenia.
