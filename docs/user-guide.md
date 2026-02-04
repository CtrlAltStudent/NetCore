# NetCore – krótki przewodnik (Beta)

## Co to jest NetCore?

NetCore to aplikacja analityczna dla e-commerce: pokazuje **realną rentowność** (zysk netto operacyjny), a nie sam obrót. Umożliwia modelowanie kosztów stałych i zmiennych, przypisywanie ich do kanałów sprzedaży, działów i pracowników oraz śledzenie marży i premii.

## Wymagania

- **Backend:** .NET 10 SDK, PostgreSQL.
- **Aplikacja (MAUI):** Windows 10/11 lub Android (zainstalowany workload MAUI).

## Uruchomienie

1. **Baza danych:** Utwórz bazę PostgreSQL (np. `NetCore`) i ustaw connection string w `src/NetCore.Api/appsettings.json` (klucz `ConnectionStrings:DefaultConnection`).
2. **API:** W katalogu `src/NetCore.Api` uruchom `dotnet run`. API będzie dostępne pod adresem z launchSettings (np. https://localhost:5001).
3. **Aplikacja MAUI:** W `src/NetCore.Maui` w pliku `MauiProgram.cs` ustaw zmienną `baseUrl` na adres API (np. `https://localhost:5001`). Następnie uruchom: `dotnet build -t:Run -f net10.0-windows10.0.19041.0` (Windows).

## Pierwsze kroki

1. **Rejestracja:** W aplikacji (ekran logowania) nie ma jeszcze przycisku „Zarejestruj” – zarejestruj się przez API (Swagger): `POST /api/v1/auth/register` z body: `{ "email": "twoj@email.pl", "password": "Haslo123!", "organizationName": "Twoja Firma" }`.
2. **Logowanie:** Zaloguj się w aplikacji tym emailem i hasłem.
3. **Dane:** Dodaj okres (np. miesiąc), kanały sprzedaży, działy, pracowników. Wprowadź przychody i koszty (z przypisaniami do kanałów/działów). Raporty i premie pojawią się w odpowiednich zakładkach.

## Zakładki w aplikacji

- **Dashboard:** Zysk operacyjny dla ostatniego okresu.
- **Raporty:** Marża i wynik per okres, per kanał, per dział.
- **Kanały / Działy / Okresy:** Listy słownikowe (odczyt).
- **Premie:** Oblicz premie dla wybranego okresu (na podstawie zdefiniowanych reguł w API).

## API (Swagger)

Przy uruchomionym API wejdź w przeglądarce na: `https://localhost:5xxx/swagger` (port z launchSettings). Tam możesz rejestrować użytkowników, dodawać okresy, kanały, przychody, koszty i reguły premii.
