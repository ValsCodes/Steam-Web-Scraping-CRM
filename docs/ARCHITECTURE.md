# Architecture Overview

## High-level system

```text
Angular Client (SteamApp.Client)
        |
        | HTTPS + Bearer JWT
        v
ASP.NET Core API (SteamApp.WebAPI)
        |
        | EF Core
        v
SQL Server
```

Additionally, the backend integrates with Steam endpoints for scraping workflows and runs background jobs for periodic tasks.

---

## Backend layers

Located under `SteamApp.Server/`:

- **SteamApp.WebAPI**
  - Application host
  - Dependency injection, auth, CORS, swagger
  - Minimal API endpoint registration + controllers
- **SteamApp.Application**
  - DTOs
  - operation result models
  - shared application contracts
- **SteamApp.Infrastructure**
  - repository/services implementations
- **SteamApp.Models**
  - domain entities/value objects
- **SteamApp.WebApiClient**
  - internal API client wrappers
- **SteamApp.Tests**
  - test project(s)

---

## Frontend organization

Located under `SteamApp.Client/src/app/`:

- `pages/` — route-level views/forms (CRUD and scraping screens)
- `services/` — HTTP clients for each API resource
- `models/` — typed client-side models
- `components/` — reusable UI components

The app uses route guards and interceptors for auth-protected navigation/API calls.

---

## Key domain entities

- Game
- GameUrl
- Product
- Pixel
- Tag
- WishList
- WatchList

### Important many-to-many relationships

- GameUrl ↔ Product
- GameUrl ↔ Pixel
- Product ↔ Tag

These are managed from primary entity forms in the current UX.

---

## API composition

The API exposes:

- **Auth** endpoint for token issuing
- **CRUD** endpoints for core entities
- **M2M relation endpoints** for relation tables
- **Steam scraping endpoints** for listing/pixel workflows

Swagger/OpenAPI is enabled by default in the WebAPI host.

---

## Security model

- JWT bearer authentication
- Authorization required on grouped endpoints
- Dedicated `InternalJob` policy for internal scope claims

---

## Background processing

The backend registers a hosted worker for wishlist checks.
Worker options are controlled through configuration (`Workers:WishlistCheck`).

---

## Configuration model

Config is loaded from:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. environment variables
4. user secrets (development)

Startup fails fast if required settings are missing.
