@echo off
setlocal EnableExtensions

title SteamApp Local Release Launcher

set "ROOT=%~dp0"
set "SERVER_DIR=%ROOT%SteamApp.Server\SteamApp.WebAPI"
set "CLIENT_DIR=%ROOT%SteamApp.Client"

where dotnet.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: The .NET SDK was not found on PATH.
    pause
    exit /b 1
)

where npm.cmd >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js/npm was not found on PATH.
    pause
    exit /b 1
)

where curl.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: curl.exe was not found on PATH.
    pause
    exit /b 1
)

where sqllocaldb.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: SQL Server LocalDB was not found on PATH.
    echo Install SQL Server Express LocalDB, then run this launcher again.
    pause
    exit /b 1
)

where sqlcmd.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: sqlcmd.exe was not found on PATH.
    echo Install the SQL Server command-line utilities, then run this launcher again.
    pause
    exit /b 1
)

sqllocaldb.exe info MSSQLLocalDB >nul 2>&1
if errorlevel 1 (
    echo ERROR: The MSSQLLocalDB instance is not installed.
    pause
    exit /b 1
)

if not exist "%SERVER_DIR%\SteamApp.WebAPI.csproj" (
    echo ERROR: The SteamApp server project was not found.
    pause
    exit /b 1
)

if not exist "%CLIENT_DIR%\package.json" (
    echo ERROR: The SteamApp client project was not found.
    pause
    exit /b 1
)

rem Build the server in Release while using the local runtime configuration.
rem Development is intentional here: it loads the existing user-secrets without
rem committing JWT/client secrets. Use the existing LocalDB database; do not
rem create a separate empty database for this launcher.
set "ASPNETCORE_ENVIRONMENT=Development"
set "DOTNET_ENVIRONMENT=Development"
set "ASPNETCORE_URLS=https://localhost:7443;http://localhost:5136"
set "LOCAL_DB_NAME=db_steam_app2"
set "ConnectionStrings__DefaultConnection=Server=(localdb)\MSSQLLocalDB;Database=%LOCAL_DB_NAME%;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
set "Database__ApplyMigrationsOnStartup=true"
set "Database__EnsureIdentitySchemaOnStartup=true"
set "API_URL=https://localhost:7443/swagger/index.html"
set "CLIENT_URL=http://localhost:4200/"

sqllocaldb.exe start MSSQLLocalDB >nul 2>&1

sqlcmd.exe -S "(localdb)\MSSQLLocalDB" -E -b -Q "IF DB_ID(N'%LOCAL_DB_NAME%') IS NULL THROW 50000, 'The existing SteamApp LocalDB database was not found.', 1;" >nul 2>&1
if errorlevel 1 (
    echo ERROR: The existing LocalDB database "%LOCAL_DB_NAME%" was not found.
    echo This launcher will not create a replacement database.
    pause
    exit /b 1
)

call :stop_owned_listener 7443 "%SERVER_DIR%" "SteamApp.WebAPI.dll"
if errorlevel 2 goto api_port_in_use
if errorlevel 1 goto api_stop_failed

echo Starting the Release API with existing LocalDB database "%LOCAL_DB_NAME%"...
start "SteamApp API - Release + LocalDB" /D "%SERVER_DIR%" cmd.exe /d /k "dotnet run --project SteamApp.WebAPI.csproj --configuration Release --no-launch-profile"

:prepare_client
call :stop_owned_listener 4200 "%CLIENT_DIR%" "SteamApp.Client"
if errorlevel 2 goto client_port_in_use
if errorlevel 1 goto client_stop_failed

echo Starting the optimized local-release Angular client...
start "SteamApp Client - Local Release" /D "%CLIENT_DIR%" cmd.exe /d /k "if exist node_modules\.bin\ng.cmd (npm.cmd run start -- --configuration local-release --host 127.0.0.1 --port 4200) else (npm.cmd ci && npm.cmd run start -- --configuration local-release --host 127.0.0.1 --port 4200)"

:wait_for_services
echo Waiting for the API and client to become ready...
call :wait_for_url "%API_URL%" 120
if errorlevel 1 goto api_start_failed

call :wait_for_url "%CLIENT_URL%" 120
if errorlevel 1 goto client_start_failed

if /I "%STEAMAPP_NO_BROWSER%"=="1" goto browser_skipped

echo Opening SteamApp in the default browser...
start "" "%CLIENT_URL%"

:browser_skipped

echo.
echo SteamApp is ready.
echo Client:  http://localhost:4200
echo API:     https://localhost:7443
echo Swagger: https://localhost:7443/swagger
echo Database: %LOCAL_DB_NAME% on ^(localdb^)\MSSQLLocalDB
echo Close the API and client terminal windows to stop services started here.

endlocal
exit /b 0

:api_port_in_use
echo.
echo ERROR: Port 7443 is owned by a process outside this SteamApp checkout.
call :show_port_owner 7443
pause
exit /b 1

:client_port_in_use
echo.
echo ERROR: Port 4200 is owned by a process outside this SteamApp checkout.
call :show_port_owner 4200
pause
exit /b 1

:api_stop_failed
echo.
echo ERROR: The existing SteamApp API on port 7443 could not be stopped.
call :show_port_owner 7443
pause
exit /b 1

:client_stop_failed
echo.
echo ERROR: The existing SteamApp client on port 4200 could not be stopped.
call :show_port_owner 4200
pause
exit /b 1

:api_start_failed
echo.
echo ERROR: The SteamApp API did not become ready within 120 seconds.
echo Check the "SteamApp API - Release + LocalDB" terminal for details.
pause
exit /b 1

:client_start_failed
echo.
echo ERROR: The SteamApp client did not become ready within 120 seconds.
echo Check the "SteamApp Client - Local Release" terminal for details.
pause
exit /b 1

:wait_for_url
for /l %%I in (1,1,%~2) do (
    curl.exe -ksf "%~1" -o NUL >nul 2>&1 && exit /b 0
    timeout.exe /t 1 /nobreak >nul
)
exit /b 1

:stop_owned_listener
powershell.exe -NoProfile -Command "$connections = @(Get-NetTCPConnection -State Listen -LocalPort %~1 -ErrorAction SilentlyContinue); if ($connections.Count -eq 0) { exit 0 }; $expected = [IO.Path]::GetFullPath('%~2'); $marker = '%~3'; $processes = @(); foreach ($processId in ($connections.OwningProcess | Sort-Object -Unique)) { $process = Get-CimInstance Win32_Process -Filter ('ProcessId=' + $processId) -ErrorAction SilentlyContinue; $path = [string]$process.ExecutablePath; $command = [string]$process.CommandLine; $owned = ($path.StartsWith($expected, [StringComparison]::OrdinalIgnoreCase) -or $command.IndexOf($expected, [StringComparison]::OrdinalIgnoreCase) -ge 0 -or $command.IndexOf($marker, [StringComparison]::OrdinalIgnoreCase) -ge 0); if (-not $owned) { exit 2 }; $processes += $processId }; foreach ($processId in $processes) { Write-Host ('Stopping existing SteamApp process on port %~1 (PID {0})...' -f $processId); Stop-Process -Id $processId -Force -ErrorAction Stop }; for ($attempt = 0; $attempt -lt 50; $attempt++) { if (-not (Get-NetTCPConnection -State Listen -LocalPort %~1 -ErrorAction SilentlyContinue)) { exit 0 }; Start-Sleep -Milliseconds 100 }; exit 1"
exit /b %errorlevel%

:show_port_owner
powershell.exe -NoProfile -Command "$connections = Get-NetTCPConnection -State Listen -LocalPort %~1 -ErrorAction SilentlyContinue; foreach ($connection in $connections) { $owner = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue; Write-Host ('  PID {0}: {1}' -f $connection.OwningProcess, $owner.ProcessName) }"
exit /b 0
