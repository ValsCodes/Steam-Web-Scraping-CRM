# Full Docker Compose stack

`docker-compose.full.yml` runs the Angular client, .NET API, SQL Server, Redis,
RabbitMQ, and the Grafana OTEL-LGTM observability image as the isolated
`steamapp-full` Compose project. It is intended for a private staging host behind
an existing HTTPS reverse proxy. The existing Compose files remain independent
alternatives.

Grafana OTEL-LGTM bundles Grafana, the OpenTelemetry Collector, Prometheus,
Loki, Tempo, and Pyroscope. Grafana documents this image for development,
demonstration, and testing rather than production-grade observability.

## Configure

From the repository root, copy the example and fill every required blank value:

```powershell
Copy-Item env\full-stack.env.example env\full-stack.env
```

Use strong URL-safe values for SQL Server, Redis, and RabbitMQ passwords so they
remain valid when embedded in provider connection strings. The two client secret
hashes must be 64-character SHA-256 hexadecimal hashes of the secrets presented
by those clients. The populated file is ignored by Git; restrict its host file
permissions because Docker passes these values to container environments.

The default bridge subnet is `172.30.50.0/24`. Change
`STEAMAPP_NETWORK_SUBNET` before first startup if it overlaps another host or VPN
network. The same value configures the API's trusted proxy network.

## Start and inspect

Validate without printing resolved credentials:

```powershell
docker compose --env-file env/full-stack.env -f docker-compose.full.yml config --quiet
```

Build and start the stack:

```powershell
docker compose --env-file env/full-stack.env -f docker-compose.full.yml up -d --build
docker compose --env-file env/full-stack.env -f docker-compose.full.yml ps
```

Follow logs without rendering the resolved Compose configuration:

```powershell
docker compose --env-file env/full-stack.env -f docker-compose.full.yml logs -f
```

Only the Angular entry point and Grafana are published, both on loopback. The
API, SQL Server, Redis, RabbitMQ, RabbitMQ management UI, and OTLP receiver are
available only on the `steamapp-network` bridge.

## Host reverse proxy

Configure the existing host proxy with two HTTPS virtual hosts:

- Route `https://<STEAMAPP_PUBLIC_HOST>` to
  `http://127.0.0.1:<STEAMAPP_HTTP_PORT>` (default `4200`).
- Route `https://<GRAFANA_PUBLIC_HOST>` to
  `http://127.0.0.1:<GRAFANA_HTTP_PORT>` (default `3000`).

For the application route, overwrite or correctly maintain `Host`,
`X-Forwarded-For`, and `X-Forwarded-Proto`. Do not pass an arbitrary client
supplied `X-Forwarded-Proto` through unchanged. The Angular Nginx container
preserves those headers and proxies `/api` and `/steam` to the internal API.

## Stop and reset

Stop containers while retaining all named volumes:

```powershell
docker compose --env-file env/full-stack.env -f docker-compose.full.yml down
```

The following command permanently deletes this stack's SQL, Redis, RabbitMQ,
and observability data. Run it only when a complete reset is intended:

```powershell
docker compose --env-file env/full-stack.env -f docker-compose.full.yml down --volumes
```

## Existing workflows

The original infrastructure, server-local, observability, and mail workflows
are unchanged:

```powershell
docker compose -f docker-compose.yml up -d
docker compose -f SteamApp.Server\SteamApp.WebAPI\docker-compose.yml up -d
docker compose -f docker-compose.observability.yml up -d
docker compose -f docker-compose.mail.yml up -d
```

Because the full stack uses a distinct project name and isolated volumes, it
does not share SQL or Redis data with the original workflows. Host port conflicts
can still occur if multiple workflows publish the same port simultaneously.
