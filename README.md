# SteamApp

A full-stack Steam market intelligence and workflow automation platform.

- **Frontend:** Angular 21 (`SteamApp.Client`)
- **Backend:** .NET 9 Web API (`SteamApp.Server/SteamApp.WebAPI`)
- **Primary use case:** connect Steam catalog context with reusable matching rules, live manual checks, automated queues, monitoring targets, and auditable run history.

SteamApp is not a traditional customer-relationship CRM. It is an operations workspace for turning repeated Steam market research into structured, explainable workflows.

> **Disclaimer**
> This project is not affiliated with, endorsed by, or sponsored by Valve Software.

## Documentation index

- [Getting Started](docs/GETTING_STARTED.md)
- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Reference](docs/API_REFERENCE.md)
- [Self-Hosted Mailserver](docs/MAILSERVER.md)
- [Contributing Guide](docs/CONTRIBUTING.md)
- [Extended Project Description](docs/PROJECT_DESCRIPTION_ENG.md)

## Quick start

### One-click local Release (Windows)

After configuring the backend user-secrets once, double-click
`Start-SteamApp-Local-Release.bat` in the repository root. It starts:

- the .NET API with `--configuration Release` on `https://localhost:7443`;
- the optimized Angular `local-release` configuration on `http://localhost:4200`;
- the existing SQL Server LocalDB database `db_steam_app2`, with automatic EF
  Core migrations.

The launcher points `DefaultConnection` to the existing database and stops with
an error if that database is missing; it does not create a replacement database.
JWT and client secrets stay in .NET user-secrets and are not written into the
batch file. It waits until both services are ready before opening the default
browser. Each run stops API/client listeners that belong to this SteamApp
checkout and starts both again, guaranteeing that the API receives the explicit
`db_steam_app2` LocalDB connection. It refuses to terminate an unrelated process
if another application owns port 7443 or 4200.

### 1) Clone

```bash
git clone <your-repository-url>
cd Steam-Web-Scraping-CRM
```

### 2) Start backend

```bash
cd SteamApp.Server/SteamApp.WebAPI
# configure secrets/env vars first (see docs/GETTING_STARTED.md)
dotnet restore
dotnet build
dotnet run
```

### 3) Start frontend

```bash
cd SteamApp.Client
npm install
npm start
```

- Frontend: `http://localhost:4200`
- Backend swagger: `https://localhost:7273/swagger` (port may vary by local profile)

## Core features

- Connected catalog management for games, Steam market URLs, products, tags, item groups, current stock, and stock history.
- Reusable manual-check criteria with saved presets, expression groups, product selection, limits, and cooldowns.
- Preset combinations using ordered top-level `AND`/`OR` operators without modifying the referenced presets.
- Live manual-check outcomes with Pending, Matched, No match, and Failed filters, plus pause, continue, cancel, history, and rerun controls.
- Automated queues composed from presets, preset combinations, private templates, and delay blocks.
- Queue definitions resolve current preset references at run start and persist frozen snapshots for historical analysis.
- Wish-list conditions and watch-list targets for ongoing market monitoring.
- Filter-first tables and Excel export in supported catalog views.
- JWT authentication, user-scoped data, protected routes, rate limits, and background workers.

Legacy pixel and standalone scraper implementations remain in the repository for compatibility, but their client routes are deprecated and they are not part of the active product navigation.

## Tech stack

### Frontend

- Angular 21
- Angular Material
- RxJS
- Tailwind + SCSS
- XLSX export

### Backend

- ASP.NET Core (.NET 9)
- Minimal APIs + Controllers
- Entity Framework Core (SQL Server)
- JWT auth
- AutoMapper
- RabbitMQ-backed and hosted background workers
- Memory and Redis caching

## Current repository layout

```text
Steam-Web-Scraping-CRM/
├─ SteamApp.Client/                  # Angular application
├─ SteamApp.Server/
│  ├─ SteamApp.WebAPI/              # ASP.NET Core API host
│  ├─ SteamApp.Application/         # DTOs / application layer
│  ├─ SteamApp.Infrastructure/      # infra services/repos
│  ├─ SteamApp.Models/              # domain layer
│  └─ SteamApp.Tests/               # tests
└─ docs/                            # project documentation
```

## Support

If you want help running the stack locally, open an issue with:

- OS + runtime versions (`node -v`, `dotnet --version`)
- backend logs
- frontend console/build output
- the exact endpoint or page you are testing
