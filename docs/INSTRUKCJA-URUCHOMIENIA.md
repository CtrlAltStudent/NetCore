# NetCore – szczegółowa instrukcja uruchomienia

Po reinstalacji środowiska lub nowym sklonowaniu repozytorium wykonaj poniższe kroki w podanej kolejności.

---

## 1. Wymagania (co musi być zainstalowane)

### 1.1 .NET 10 SDK

- Pobierz i zainstaluj z: https://dotnet.microsoft.com/download/dotnet/10.0  
- W terminalu (PowerShell lub CMD) sprawdź wersję:
  ```powershell
  dotnet --version
  ```
  Powinna być **10.x.x** (np. 10.0.100).

### 1.2 Workload MAUI (dla aplikacji desktopowej)

- W terminalu uruchom:
  ```powershell
  dotnet workload install maui
  ```
- Na Windows potrzebny jest też workload **Windows** (zazwyczaj dołączony do MAUI). Jeśli MAUI na Windows nie buduje się, wykonaj:
  ```powershell
  dotnet workload install maui
  dotnet workload list
  ```
  i upewnij się, że na liście jest `maui-windows`.

### 1.3 PostgreSQL (dla API)

- Zainstaluj PostgreSQL (np. z https://www.postgresql.org/download/windows/ ).
- Zapamiętaj:
  - **hasło** ustawione dla użytkownika `postgres`,
  - **port** (domyślnie 5432).

### 1.4 (Opcjonalnie) Android SDK i JDK

- Pełne budowanie **całego** rozwiązania (`dotnet build NetCore.slnx`) wymaga Android SDK i JDK tylko do targetu **Android**.
- **Do uruchomienia API i aplikacji MAUI na Windows nie są potrzebne** – możesz je pominąć, jeśli korzystasz tylko z Windows.

---

## 2. Baza danych PostgreSQL

### 2.1 Uruchom PostgreSQL

- Upewnij się, że usługa PostgreSQL działa (np. „PostgreSQL” w usługach Windows lub uruchomiony serwer w tle).

### 2.2 Utwórz bazę i użytkownika (jeśli jeszcze nie ma)

- Otwórz **pgAdmin** lub **psql** (linia poleceń PostgreSQL).
- Utwórz bazę, np.:
  ```sql
  CREATE DATABASE NetCore;
  ```
- Domyślny użytkownik to często `postgres`; jeśli używasz innego, utwórz go i nadaj uprawnienia do bazy `NetCore`.

### 2.3 Connection string w projekcie

- Otwórz plik:
  ```
  c:\Users\mskaw\Desktop\NetCore\src\NetCore.Api\appsettings.json
  ```
- Ustaw `ConnectionStrings:DefaultConnection` zgodnie ze swoim środowiskiem, np.:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NetCore;Username=postgres;Password=TWOJE_HASLO"
  }
  ```
  Zamień `TWOJE_HASLO` na hasło użytkownika PostgreSQL.  
  Jeśli port to nie 5432, zmień `Port=5432`.  
  Jeśli baza ma inną nazwę, zmień `Database=NetCore`.

---

## 3. Budowanie projektu

### 3.1 Budowanie tylko API (bez MAUI dla Androida)

W **PowerShell** lub **CMD**:

```powershell
cd c:\Users\mskaw\Desktop\NetCore\src\NetCore.Api
dotnet restore
dotnet build
```

Oczekiwany wynik: **Kompilacja powiodła się**, 0 błędów.

### 3.2 Budowanie MAUI dla Windows

```powershell
cd c:\Users\mskaw\Desktop\NetCore\src\NetCore.Maui
dotnet restore
dotnet build -f net10.0-windows10.0.19041.0
```

Oczekiwany wynik: **Kompilacja powiodła się**, 0 błędów.

### 3.3 (Opcjonalnie) Budowanie całego rozwiązania

- Na Windows pełne `dotnet build NetCore.slnx` zbuduje też target **Android** – to się **nie uda**, jeśli nie masz zainstalowanego Android SDK i JDK.  
- Możesz to zignorować i używać tylko API + MAUI dla Windows (kroki 3.1 i 3.2).

---

## 4. Uruchomienie API

### 4.1 Uruchom serwer API

W terminalu:

```powershell
cd c:\Users\mskaw\Desktop\NetCore\src\NetCore.Api
dotnet run
```

Albo z wyborem profilu (HTTP lub HTTPS):

```powershell
dotnet run --launch-profile https
```

Poczekaj, aż w logach pojawi się informacja o nasłuchiwaniu, np.:

- `Now listening on: https://localhost:7031`
- `Now listening on: http://localhost:5174`

**Nie zamykaj tego okna terminala** – API musi cały czas działać.

### 4.2 Adresy API

- **HTTPS:** https://localhost:7031  
- **HTTP:** http://localhost:5174  
- **Swagger (dokumentacja):** https://localhost:7031/swagger lub http://localhost:5174/swagger  

Dokładne porty sprawdzisz w:

```
src\NetCore.Api\Properties\launchSettings.json
```

(w polu `applicationUrl` wybranego profilu).

### 4.3 Migracje bazy

- Przy pierwszym uruchomieniu `dotnet run` aplikacja sama uruchamia migracje EF Core (`Database.MigrateAsync()`).  
- Jeśli baza jest pusta, tabele zostaną utworzone automatycznie.  
- W razie błędu (np. brak połączenia z PostgreSQL) sprawdź connection string i czy serwer PostgreSQL działa.

---

## 5. Uruchomienie aplikacji MAUI (Windows)

### 5.1 Adres API w MAUI

W pliku `src\NetCore.Maui\MauiProgram.cs` jest ustawiony adres API:

```csharp
var baseUrl = "https://localhost:7031";
```

Powinien być **taki sam** jak adres HTTPS z `launchSettings.json` (domyślnie 7031). Jeśli uruchamiasz API na innym porcie, zmień `baseUrl` w `MauiProgram.cs`.

### 5.2 Uruchom aplikację

**W drugim terminalu** (pierwszy ma działać z `dotnet run` w Api):

```powershell
cd c:\Users\mskaw\Desktop\NetCore\src\NetCore.Maui
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

Alternatywnie (jeśli projekt jest już zbudowany):

```powershell
dotnet run -f net10.0-windows10.0.19041.0
```

Powinno otworzyć się okno aplikacji NetCore (ekran logowania).

### 5.3 Uruchomienie z poziomu Visual Studio / Cursor

- Otwórz rozwiązanie (np. `NetCore.slnx`).
- Ustaw **NetCore.Api** jako projekt startowy, uruchom (F5) – API będzie działać.
- Potem ustaw **NetCore.Maui** jako projekt startowy, w konfiguracji uruchomienia wybierz **Windows Machine** i uruchom (F5).

---

## 6. Pierwsze użycie

### 6.1 Rejestracja użytkownika

- W aplikacji MAUI na ekranie logowania może nie być przycisku „Zarejestruj”.
- Zarejestruj się przez **Swagger**:  
  1. Otwórz w przeglądarce: https://localhost:7031/swagger  
  2. Znajdź **POST /api/v1/auth/register**  
  3. Kliknij „Try it out”, w body wpisz np.:
     ```json
     {
       "email": "twoj@email.pl",
       "password": "Haslo123!",
       "organizationName": "Twoja Firma"
     }
     ```
  4. Wykonaj żądanie (Execute).

### 6.2 Logowanie w aplikacji

- W aplikacji MAUI wpisz ten sam **email** i **hasło** i zaloguj się.

### 6.3 Dane testowe (szybki test bez ręcznego wpisywania)

- Na **Dashboardzie** w aplikacji MAUI jest przycisk **„Załaduj dane testowe”**. Kliknij go (gdy organizacja jest pusta – nie ma jeszcze okresów). Aplikacja utworzy w API jeden okres, 3 kanały (Sklep własny, Allegro, Amazon), 2 działy (Sprzedaż, Logistyka), pracowników, przychody i koszty. Dashboard i zakładki Raporty / Kanały / Działy / Okresy od razu pokażą dane przykładowego biznesu.
- Endpoint API: **POST /api/v1/seed/test-data** (wymaga zalogowania). Można go wywołać też ze Swaggera. Działa tylko, gdy organizacja nie ma jeszcze żadnych okresów.

### 6.4 Dalsze kroki

- Dodaj własne okresy, kanały, działy, pracowników przez API (Swagger) lub w przyszłości z poziomu aplikacji.
- Wprowadź przychody i koszty – raporty i premie będą dostępne w odpowiednich zakładkach.

---

## 7. Podsumowanie – szybka checklista

| Krok | Akcja |
|------|--------|
| 1 | Zainstaluj .NET 10 SDK (`dotnet --version` = 10.x) |
| 2 | Zainstaluj workload MAUI: `dotnet workload install maui` |
| 3 | Zainstaluj i uruchom PostgreSQL, utwórz bazę `NetCore` |
| 4 | W `src/NetCore.Api/appsettings.json` ustaw `ConnectionStrings:DefaultConnection` |
| 5 | `cd src/NetCore.Api` → `dotnet run` (zostaw włączone) |
| 6 | W drugim terminalu: `cd src/NetCore.Maui` → `dotnet build -t:Run -f net10.0-windows10.0.19041.0` |
| 7 | W przeglądarce: https://localhost:7031/swagger – zarejestruj użytkownika (POST /api/v1/auth/register) |
| 8 | Zaloguj się w aplikacji MAUI emailem i hasłem |

---

## 8. Typowe problemy

- **„Plik jest zablokowany przez: NetCore.Api (…)” przy `dotnet run` w NetCore.Api** – oznacza to, że **API już działa** w innym oknie terminala (np. wcześniej uruchomione i nie zamknięte). Nie uruchamiaj drugiej kopii. Albo **użyj tej działającej instancji** (aplikacja MAUI się z nią połączy), albo **zatrzymaj ją**: w oknie PowerShell/CMD, gdzie API działa, naciśnij **Ctrl+C**, a potem w bieżącym oknie ponownie wpisz `dotnet run`. Możesz też zakończyć proces z Menedżera zadań (szukaj „NetCore.Api”) lub w PowerShell: `Stop-Process -Id 16868 -Force` (podstaw numer PID z komunikatu błędu).
- **„Błąd inicjalizacji” w okienku MAUI** – na Windows kontener DI bywał niedostępny przy starcie. W projekcie jest to naprawione (użycie `App.Services`). Jeśli nadal widzisz błąd, w treści komunikatu sprawdź dokładny opis (np. brak połączenia z API). Upewnij się, że **najpierw uruchomiono API** (`dotnet run` w `NetCore.Api`), a w `MauiProgram.cs` jest ustawiony właściwy `baseUrl` (np. `https://localhost:7031`).
- **„Nie można odnaleźć katalogu zestawu Android SDK”** – dotyczy tylko budowania targetu Android. Używaj budowania tylko dla Windows: `dotnet build -f net10.0-windows10.0.19041.0` w projekcie MAUI.
- **Błąd połączenia z bazą** – sprawdź czy PostgreSQL działa, connection string w `appsettings.json` oraz czy baza `NetCore` istnieje.
- **Aplikacja MAUI nie łączy się z API** – sprawdź czy `baseUrl` w `MauiProgram.cs` jest taki sam jak adres z `launchSettings.json` (np. `https://localhost:7031`) i czy API rzeczywiście działa w tym samym momencie.
- **Błąd 404 przy „Załaduj dane testowe”** – działająca instancja API została uruchomiona **przed** dodaniem endpointu seed. Zatrzymaj API (Ctrl+C w terminalu, gdzie działa), w folderze `src/NetCore.Api` wykonaj `dotnet run` ponownie. W przeglądarce wejdź na http://localhost:5174/swagger i sprawdź, czy na liście jest **POST /api/v1/seed/test-data**.
- **Certyfikat HTTPS** – przy pierwszym wejściu na https://localhost:7031 przeglądarka może ostrzegać o certyfikacie deweloperskim; w środowisku lokalnym zwykle można to zaakceptować.

---

Więcej informacji: [user-guide.md](user-guide.md) oraz [api-overview.md](api-overview.md).
