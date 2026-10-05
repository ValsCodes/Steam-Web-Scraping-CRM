using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace SteamApp.WebAPI.Observability;

internal static class ObservabilityExtensions
{
    internal static void AddSteamAppObservability(this WebApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("Observability:Enabled"))
        {
            return;
        }

        var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "steamapp-api";
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes([
                    new KeyValuePair<string, object>(
                        "deployment.environment.name",
                        builder.Environment.EnvironmentName)
                ]))
            .WithTracing(tracing => tracing
                .AddSource(
                    SteamAppTelemetry.ActivitySourceName,
                    RabbitMQActivitySource.PublisherSourceName,
                    RabbitMQActivitySource.SubscriberSourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = false;
                })
                .AddProcessor(new TelemetryPrivacyProcessor())
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddMeter(SteamAppTelemetry.MeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = false;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.AddProcessor(new TelemetryLogPrivacyProcessor());
            logging.AddOtlpExporter();
        });
    }
}
