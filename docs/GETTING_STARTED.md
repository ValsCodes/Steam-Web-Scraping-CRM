# Getting Started

This guide covers local setup for both backend and frontend.

## Prerequisites

- **Node.js** latest LTS (recommended)
- **npm** (bundled with Node)
- **.NET 9 SDK**
- **SQL Server** instance (local or remote)
- Optional: EF Core tools (`dotnet-ef`) if you apply migrations manually

---

## 1. Clone repository

```bash
git clone <your-repository-url>
cd Steam-Web-Scraping-CRM
```

---

## 2. Configure backend secrets/config

The API validates required configuration keys at startup.

Required keys:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:Key`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `InternalClient:ClientSecret`

You can set these using **User Secrets** (recommended in development) and/or environment variables.

### Option A — User Secrets (recommended)

```bash
cd SteamApp.Server/SteamApp.WebAPI
# initialize once if needed
dotnet user-secrets init

# examples
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=SteamAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Key" "<very-long-random-key>"
dotnet user-secrets set "JwtSettings:Issuer" "SteamApp"
dotnet user-secrets set "JwtSettings:Audience" "SteamAppClient"
dotnet user-secrets set "InternalClient:ClientSecret" "<secret>"
```

### Option B — Environment variables

Set equivalent variables using `__` as section separator, for example:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Key`

---

## 3. Backend startup

```bash
cd SteamApp.Server/SteamApp.WebAPI
dotnet restore
dotnet build
dotnet run
```

Swagger should be available at `/swagger` on the backend URL.

---

## 4. Database migrations (if needed)

If your environment needs migrations applied manually:

```bash
cd SteamApp.Server/SteamApp.WebAPI
dotnet ef database update
```

> If EF tools are not installed: `dotnet tool install --global dotnet-ef`

---

## 5. Frontend startup

```bash
cd SteamApp.Client
npm install
npm start
```

The Angular app runs on `http://localhost:4200` by default.

---

## 6. Authentication flow

Most API endpoints require JWT auth. Typical flow:

1. Call `POST /api/Auth/token`
2. Use returned bearer token in `Authorization: Bearer <token>`
3. Access protected endpoints

---

## 7. Common troubleshooting

### API throws “Missing required configuration”

One of required keys is missing or empty. Re-check secrets/env vars.

### Angular production build fails due Google Fonts inlining

If environment/network blocks fonts.googleapis.com, production build can fail with 403 during font inlining. Use development build (`ng build --configuration development`) for local verification in restricted environments.

### CORS/auth issues

- Confirm backend is running
- Confirm frontend points to correct API base URL
- Ensure bearer token is set and not expired

---

## 8. Useful commands

### Frontend

```bash
npm start
npm run build -- --configuration development
npm test
```

### Backend

```bash
dotnet restore
dotnet build
dotnet test
```
