# API overview

REST API wersja 1: prefix `/api/v1/`. Wszystkie endpointy (oprócz auth) wymagają nagłówka `Authorization: Bearer <token>`.

- **Auth:** `POST /api/v1/auth/register`, `POST /api/v1/auth/login`
- **Słowniki:** CRUD `/api/v1/sales-channels`, `/api/v1/departments`, `/api/v1/employees`, `/api/v1/periods`
- **Dane:** CRUD `/api/v1/revenues`, `/api/v1/costs` (przypisania w body kosztu)
- **Raporty:** GET `/api/v1/reports/margin?periodId=`, `/api/v1/reports/operating-result?periodId=`, `/api/v1/reports/by-channel?periodId=`, `/api/v1/reports/by-department?periodId=`
- **Premie:** CRUD `/api/v1/bonus-rules`, GET `/api/v1/bonuses/calculate?periodId=`

Dokumentacja interaktywna: Swagger UI przy uruchomionym API (`/swagger`).
