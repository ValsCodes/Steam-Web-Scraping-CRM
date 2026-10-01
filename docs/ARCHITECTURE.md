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

Additionally, the backend integrates with Steam listing sources, RabbitMQ, Redis, and hosted workers for long-running checks, queue execution, notifications, and periodic tasks.

The active product surface is organized around four domains:

1. connected market context and stock;
2. reusable manual-check rules and preset combinations;
3. observable run execution and frozen history;
4. automated queues and monitoring targets.

---

## Backend layers

Located under `SteamApp.Server/`:

- **SteamApp.WebAPI**
  - Application host
  - Dependency injection, auth, CORS, swagger
  - Minimal API endpoint registration + controllers
  - manual-check and automatic-queue orchestration
  - RabbitMQ messages, consumers, hosted workers, and cache composition
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

- `pages/` — route-level catalog, checks, queue, monitoring, profile, and public screens
- `services/` — HTTP clients for each API resource
- `models/` — typed client-side models
- `components/` — reusable UI components

The app uses route guards and interceptors for auth-protected navigation/API calls.

---

## Key domain entities

- Game
- GameUrl
- Product
- Tag
- ItemGroup
- WishList
- WatchList
- ManualCheckPreset
- ManualCheckRun
- AutomaticQueueDefinition
- AutomaticQueueRun

### Important many-to-many relationships

- GameUrl ↔ Product
- Product ↔ Tag

These are managed from primary entity forms in the current UX.

Pixel records and GameUrl ↔ Pixel relations remain in the server model for legacy compatibility, but their standalone client routes are deprecated.

---

## API composition

The API exposes:

- **Auth** endpoint for token issuing
- **CRUD** endpoints for active catalog and monitoring entities
- **M2M relation endpoints** for relation tables
- **Manual-check endpoints** for operators, presets, runs, controls, history, and reruns
- **Automatic-queue endpoints** for definitions, block recipes, runs, controls, and history
- **Item-group and stock endpoints** used by matching and catalog workflows

Swagger/OpenAPI is enabled by default in the WebAPI host.

Legacy pixel CRUD, pixel-relation, and direct scraping endpoints are retained for compatibility. They are not advertised as active product capabilities and should not be used as the basis for new client workflows.

---

## Security model

- JWT bearer authentication
- Authorization required on grouped endpoints
- Dedicated `InternalJob` policy for internal scope claims
- User ownership checks for presets, runs, queue definitions, and referenced multi-ID inputs
- Separate rate-limit policies for general and expensive operations

---

## Background processing

The backend uses hosted workers and RabbitMQ consumers for manual checks, automatic queues, wishlist checks, and notifications. Long-running work is detached from the initiating HTTP request, while persisted status and history keep progress observable.

Manual-check runs persist their resolved setup and flattened criteria. Automated queue definitions keep live references to saved presets, resolve them again when a run starts, and freeze the resolved block setup into that queue run. Historical combined checks therefore replay the recorded snapshot instead of silently adopting later preset edits.

---

## Configuration model

Config is loaded from:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. environment variables
4. user secrets (development)

Startup fails fast if required settings are missing.
