# Getting Started

This guide covers local setup for both backend and frontend.

## Prerequisites

- **Node.js** latest LTS (recommended)
- **npm** (bundled with Node)
- **.NET 11 SDK**
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
dotnet ef database update --project ../SteamApp.Infrastructure --startup-project SteamApp.WebAPI.csproj
```

> If EF tools are not installed: `dotnet tool install --global dotnet-ef --version 10.0.15`

---

## 5. Frontend startup

```bash
cd SteamApp.Client
npm install
npm start
```

The Angular app runs on `https://localhost:4300` by default.

---

## 6. Authentication flow

Most API endpoints require JWT auth. Typical flow:

1. Call `POST /api/Auth/token`
2. Use returned bearer token in `Authorization: Bearer <token>`
3. Access protected endpoints

### Local Google and Steam sign-in

1. Start the API with `dotnet run --project SteamApp.Server/SteamApp.WebAPI/SteamApp.WebAPI.csproj --launch-profile https` from the repository root. The API uses `https://localhost:7443`.
2. In `SteamApp.Client`, run `npm start` and open `https://localhost:4300/login`. Trust the local development certificates before testing. Use `localhost` for both origins.
3. For Google, create a **Web application** client in Google Auth Platform and add `https://localhost:7443/signin-google` under authorized redirect URIs. Set `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` in the API project's .NET User Secrets. The Google button appears when both values are configured.
4. Steam is enabled in the development configuration and uses `https://localhost:7443/signin-steam`. Browser sign-in requires a Steam account. For a new account, configure the existing `Email` SMTP settings with an authenticated local SMTP capture service or a test mailbox, then enter the code sent by SteamApp.
5. To attach a provider to an existing account, sign in to that account and select **Connect Google** or **Connect Steam** in **Profile** within five minutes.

For local registration without an existing SMTP account, start Docker Desktop and run
`.\Start-SteamApp-Local-Release.bat --local-mail` from the repository root in PowerShell.
The launcher starts a local Mailpit inbox at `http://127.0.0.1:8025` and configures the API
process to send verification codes there. To stop Mailpit, run
`docker compose --project-directory . -f docker/docker-compose.local-mail.yml down`.

Existing administrator role assignments carry forward. Where configuration is used to assign administrators, set `Authentication:AdminUserIds` to the confirmed SteamApp user IDs. The old `Authentication:AdminEmails` setting has no effect.

### Authenticator MFA

1. In **Profile**, select **Set up authenticator** within five minutes of signing in. Scan the QR code in an authenticator app, or enter the setup key manually.
2. Enter a six-digit code from the app to confirm enrollment. Save the ten backup codes before closing the confirmation. Each works once.
3. On your next password, Google, or Steam sign-in, enter an authenticator code or select **Use a backup code**. If you lose your authenticator, use a backup code to sign in, then disable MFA and enroll the new device in Profile. You need an authenticator or a remaining backup code to recover access.

Profile shows how many backup codes remain and lets you replace the entire set. Enabling or disabling MFA signs out other sessions. Changing your password requires signing in again. Sessions issued before this feature also require a fresh sign-in.

Keep the ASP.NET Core Data Protection key ring with your local database backups so authenticator secrets remain readable. Local host development uses the current user's ASP.NET Core key ring by default. Full Docker Compose uses the `api-data-protection-keys` volume. Preserve that volume when recreating the API container. For a custom container setup, set `Authentication__DataProtectionKeyPath` to a persistent directory accessible only to the API user. Pending sign-in challenges expire on API restart.

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
