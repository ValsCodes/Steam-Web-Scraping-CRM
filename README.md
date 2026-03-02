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
- [Contributing Guide](docs/CONTRIBUTING.md)

## Quick start

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
