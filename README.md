# Steam Web Scraping CRM

A full-stack application for tracking and analyzing Steam Community Market listings.

- **Frontend:** Angular 21 (`SteamApp.Client`)
- **Backend:** .NET 9 Web API (`SteamApp.Server/SteamApp.WebAPI`)
- **Primary use case:** maintain game/product/tag/pixel metadata and run market scraping workflows with watch/wish list automation.

> **Disclaimer**
> This project is not affiliated with, endorsed by, or sponsored by Valve Software.

## Documentation index

- [Getting Started](docs/GETTING_STARTED.md)
- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Reference](docs/API_REFERENCE.md)
- [Self-Hosted Mailserver](docs/MAILSERVER.md)
- [Contributing Guide](docs/CONTRIBUTING.md)
- [Project Detailed Description](docs/PROJECT_DESCRIPTION.md)

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

- CRUD for Games, Game URLs, Products, Pixels, Tags, Wish List, Watch List.
- M2M relation management from primary forms:
  - Game URL ↔ Products
  - Game URL ↔ Pixels
  - Product ↔ Tags
- Data-table filters and export to Excel in key pages.
- JWT auth and protected API routes.
- Background worker support for wishlist checks.

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
- Hosted background worker(s)

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
