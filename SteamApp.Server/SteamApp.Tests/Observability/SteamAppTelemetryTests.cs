using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SteamApp.WebAPI.Observability;

namespace SteamApp.Tests.Observability;

public sealed class SteamAppTelemetryTests
{
    [Test]
    public void AddSteamAppObservability_Disabled_DoesNotRegisterProviders()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Observability:Enabled"] = "false";

        builder.AddSteamAppObservability();

        Assert.Multiple(() =>
        {
            Assert.That(
                builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TracerProvider)),
                Is.False);
            Assert.That(
                builder.Services.Any(descriptor => descriptor.ServiceType == typeof(MeterProvider)),
                Is.False);
        });
    }

    [Test]
    public void AddSteamAppObservability_Enabled_RegistersProviders()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Observability:Enabled"] = "true";
        builder.Configuration["OTEL_SERVICE_NAME"] = "steamapp-tests";

        builder.AddSteamAppObservability();

        Assert.Multiple(() =>
        {
            Assert.That(
                builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TracerProvider)),
                Is.True);
            Assert.That(
                builder.Services.Any(descriptor => descriptor.ServiceType == typeof(MeterProvider)),
                Is.True);
        });
    }

    [Test]
    public void CompleteOperation_Error_EmitsCorrelatedSpanAndLowCardinalityMetrics()
    {
        const string operation = "test.operation";
        var stoppedActivities = new List<Activity>();
        var measurements = new List<(
            string Name,
            double Value,
            KeyValuePair<string, object?>[] Tags)>();

        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SteamAppTelemetry.ActivitySourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = stoppedActivities.Add
        };
        ActivitySource.AddActivityListener(activityListener);

        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == SteamAppTelemetry.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };
        meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            measurements.Add((instrument.Name, value, tags.ToArray())));
        meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            measurements.Add((instrument.Name, value, tags.ToArray())));
        meterListener.Start();

        using (var activity = SteamAppTelemetry.StartOperation(operation))
        {
            SteamAppTelemetry.MarkError(activity);
            SteamAppTelemetry.CompleteOperation(
                activity,
                operation,
                SteamAppTelemetry.ErrorOutcome,
                TimeSpan.FromSeconds(2));
        }

        var count = measurements.Single(x =>
            x.Name == "steamapp.background.operation.count");
        var duration = measurements.Single(x =>
            x.Name == "steamapp.background.operation.duration");

        Assert.Multiple(() =>
        {
            Assert.That(stoppedActivities, Has.Count.EqualTo(1));
            Assert.That(stoppedActivities[0].OperationName, Is.EqualTo(operation));
            Assert.That(stoppedActivities[0].Status, Is.EqualTo(ActivityStatusCode.Error));
            Assert.That(
                stoppedActivities[0].GetTagItem("steamapp.outcome"),
                Is.EqualTo(SteamAppTelemetry.ErrorOutcome));
            Assert.That(count.Value, Is.EqualTo(1));
            Assert.That(duration.Value, Is.EqualTo(2));
            Assert.That(
                count.Tags.Select(tag => tag.Key),
                Is.EquivalentTo(["steamapp.operation", "steamapp.outcome"]));
            Assert.That(
                duration.Tags.Select(tag => tag.Key),
                Is.EquivalentTo(["steamapp.operation", "steamapp.outcome"]));
        });
    }

    [Test]
    public void RecordQueueDelay_FutureRequest_ClampsMeasurementToZero()
    {
        var measurements = new List<(
            double Value,
            KeyValuePair<string, object?>[] Tags)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == SteamAppTelemetry.MeterName &&
                    instrument.Name == "steamapp.messaging.queue.delay")
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };
        listener.SetMeasurementEventCallback<double>((_, value, tags, _) =>
            measurements.Add((value, tags.ToArray())));
        listener.Start();

        var observedAtUtc = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
        SteamAppTelemetry.RecordQueueDelay(
            "test.queue",
            observedAtUtc.AddSeconds(10),
            observedAtUtc);

        Assert.Multiple(() =>
        {
            Assert.That(measurements, Has.Count.EqualTo(1));
            Assert.That(measurements[0].Value, Is.Zero);
            Assert.That(measurements[0].Tags, Has.Length.EqualTo(1));
            Assert.That(
                measurements[0].Tags[0],
                Is.EqualTo(new KeyValuePair<string, object?>(
                    "messaging.destination.name",
                    "test.queue")));
        });
    }

    [Test]
    public void CompleteOperation_UsesOutcomeMarkedByHandler()
    {
        string? metricOutcome = null;
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SteamAppTelemetry.ActivitySourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(activityListener);

        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == SteamAppTelemetry.MeterName &&
                    instrument.Name == "steamapp.background.operation.count")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };
        meterListener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag.Key == "steamapp.outcome")
                {
                    metricOutcome = tag.Value as string;
                }
            }
        });
        meterListener.Start();

        using var activity = SteamAppTelemetry.StartOperation("test.skipped");
        SteamAppTelemetry.MarkSkipped(activity);
        SteamAppTelemetry.CompleteOperation(
            activity,
            "test.skipped",
            SteamAppTelemetry.SuccessOutcome,
            TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(
                activity?.GetTagItem("steamapp.outcome"),
                Is.EqualTo(SteamAppTelemetry.SkippedOutcome));
            Assert.That(metricOutcome, Is.EqualTo(SteamAppTelemetry.SkippedOutcome));
        });
    }

    [Test]
    public void PrivacyProcessor_RemovesSqlTextAndRequestTargets()
    {
        using var activity = new Activity("privacy-test").Start();
        activity.SetTag("db.query.text", "select secret from users");
        activity.SetTag("url.full", "https://example.test/private?token=secret");
        activity.SetTag("url.path", "/private");
        activity.SetTag("url.query", "?token=secret");
        activity.SetTag("http.response.status_code", 200);

        new TelemetryPrivacyProcessor().OnEnd(activity);

        var tags = activity.TagObjects.ToDictionary(tag => tag.Key, tag => tag.Value);
        Assert.Multiple(() =>
        {
            Assert.That(tags, Does.Not.ContainKey("db.query.text"));
            Assert.That(tags, Does.Not.ContainKey("url.full"));
            Assert.That(tags, Does.Not.ContainKey("url.path"));
            Assert.That(tags, Does.Not.ContainKey("url.query"));
            Assert.That(tags["http.response.status_code"], Is.EqualTo(200));
        });
    }

    [Test]
    public void LogPrivacyProcessor_RemovesRenderedValuesAndExceptions()
    {
        var capture = new CapturingLogProcessor();
        using var loggerFactory = LoggerFactory.Create(logging =>
            logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.ParseStateValues = true;
                options.AddProcessor(new TelemetryLogPrivacyProcessor());
                options.AddProcessor(capture);
            }));
        var logger = loggerFactory.CreateLogger<SteamAppTelemetryTests>();

        logger.LogError(
            new InvalidOperationException("https://example.test/private?token=secret"),
            "User {UserId} failed with correlation {CorrelationId}.",
            "private-user",
            "safe-correlation");

        Assert.Multiple(() =>
        {
            Assert.That(capture.FormattedMessage, Is.Null);
            Assert.That(capture.Exception, Is.Null);
            Assert.That(capture.Body, Is.EqualTo(
                "User {UserId} failed with correlation {CorrelationId}."));
            Assert.That(capture.Attributes, Does.Not.ContainKey("UserId"));
            Assert.That(
                capture.Attributes["CorrelationId"],
                Is.EqualTo("safe-correlation"));
        });
    }

    private sealed class CapturingLogProcessor : BaseProcessor<LogRecord>
    {
        internal string? FormattedMessage { get; private set; }
        internal Exception? Exception { get; private set; }
        internal string? Body { get; private set; }
        internal IReadOnlyDictionary<string, object?> Attributes { get; private set; } =
            new Dictionary<string, object?>();

        public override void OnEnd(LogRecord logRecord)
        {
            FormattedMessage = logRecord.FormattedMessage;
            Exception = logRecord.Exception;
            Body = logRecord.Body;
            Attributes = logRecord.Attributes?.ToDictionary(
                attribute => attribute.Key,
                attribute => attribute.Value) ?? new Dictionary<string, object?>();
        }
    }
}
