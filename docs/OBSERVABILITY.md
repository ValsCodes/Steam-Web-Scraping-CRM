# Local observability

SteamApp can export backend logs, metrics, and traces to Grafana's all-in-one
`docker-otel-lgtm` development image. Observability is disabled by default and
is not required for normal API startup or operation.

The image uses Prometheus for metrics, Tempo for traces, Loki for logs, and an
OpenTelemetry Collector for ingestion. Mimir, production hosting, alerting,
profiling, and long-term retention are intentionally outside this local setup.

## Start the stack

From the repository root:

```powershell
docker compose -f docker-compose.observability.yml up -d
docker compose -f docker-compose.observability.yml ps
```

Wait until `otel-lgtm` reports healthy, then run the API with the explicit
observability profile:

```powershell
dotnet run --project SteamApp.Server\SteamApp.WebAPI\SteamApp.WebAPI.csproj --launch-profile https-observability
```

Open `http://127.0.0.1:3000`. The image's local default login is `admin` / `admin`.
Both Grafana and the OTLP receiver are bound to loopback and must not be exposed
through a public port forward or reverse proxy.

The normal `http`, `https`, IIS, and Docker launch profiles keep
`Observability:Enabled` false. Starting SteamApp without this container remains
the supported default.

## Guided Grafana Explore exercises

### Logs and trace correlation

1. Make an API request or run a manual check.
2. Open **Explore** and select the Loki data source.
3. filter for the `steamapp-api` service using the label browser.
4. Expand a structured log record and locate its trace and span identifiers.
5. Follow the trace link, or copy the trace identifier into the Tempo search.

### HTTP and runtime metrics

1. Open **Explore** and select Prometheus.
2. Use metric autocomplete to find `http_server_request_duration` metrics.
3. Compare request counts, duration buckets, and status-code attributes while
   sending successful and failing requests.
4. Inspect `process_runtime_dotnet` and `dotnet` metrics to explore GC,
   allocation, thread-pool, and runtime behavior exposed by the installed SDK.

### Background work and messaging

1. Trigger a manual check or an automatic queue run.
2. Search Tempo for service `steamapp-api` and operations prefixed with
   `manual-check` or `automatic-queue`.
3. With RabbitMQ enabled, trigger a scrape or wishlist flow and inspect the
   RabbitMQ producer/subscriber spans and their nested SteamApp processing span.
4. In Prometheus, explore:
   - `steamapp_background_operation_count`
   - `steamapp_background_operation_duration`
   - `steamapp_messaging_queue_delay`

Telemetry intentionally omits request and message bodies, authentication
headers, SQL statements and parameters, email addresses, Redis keys, scrape
URLs, credentials, and tokens. Exported logs retain their message template and
safe structured fields, but omit rendered values and exception details so those
channels cannot reintroduce sensitive request targets or identifiers.

## Stop and reset

```powershell
docker compose -f docker-compose.observability.yml down
```

No data volume is mounted. Removing the container removes its logs, traces, and
metrics, which keeps this learning environment disposable.

To confirm the reset behavior, create some telemetry, note a trace or log in
Explore, run `down`, start the stack again, and verify that the old trace or log
is no longer available.

## Troubleshooting

- If port 3000 or 4318 is already in use, stop the conflicting local process;
  do not change the bindings to a public interface.
- If Grafana is empty, confirm the API was started with
  `--launch-profile https-observability` and that the container is healthy.
- If the collector is stopped while the API is running, exporters may report
  delivery failures and drop telemetry. Application requests and workers must
  continue independently.
- The image is large and runs several processes. Allow Docker enough memory and
  startup time before diagnosing an empty data source.
