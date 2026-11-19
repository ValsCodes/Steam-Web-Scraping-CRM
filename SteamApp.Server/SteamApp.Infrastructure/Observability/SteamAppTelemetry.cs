using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SteamApp.Infrastructure.Observability;

public static class SteamAppTelemetry
{
    public const string ActivitySourceName = "SteamApp.WebAPI";
    public const string MeterName = "SteamApp.WebAPI";
    public const string SuccessOutcome = "success";
    internal const string SkippedOutcome = "skipped";
    public const string CancelledOutcome = "cancelled";
    public const string ErrorOutcome = "error";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> BackgroundOperationCount =
        Meter.CreateCounter<long>("steamapp.background.operation.count", "{operation}");
    private static readonly Histogram<double> BackgroundOperationDuration =
        Meter.CreateHistogram<double>("steamapp.background.operation.duration", "s");
    private static readonly Histogram<double> MessagingQueueDelay =
        Meter.CreateHistogram<double>("steamapp.messaging.queue.delay", "s");

    public static Activity? StartOperation(string operation)
    {
        var activity = ActivitySource.StartActivity(operation, ActivityKind.Internal);
        activity?.SetTag("steamapp.operation", operation);
        return activity;
    }

    public static void MarkError(Activity? activity)
    {
        activity?.SetTag("steamapp.outcome", ErrorOutcome);
        activity?.SetStatus(ActivityStatusCode.Error);
    }

    internal static void MarkSkipped(Activity? activity)
    {
        activity?.SetTag("steamapp.outcome", SkippedOutcome);
    }

    public static void CompleteOperation(
        Activity? activity,
        string operation,
        string outcome,
        TimeSpan duration)
    {
        var effectiveOutcome = activity?.GetTagItem("steamapp.outcome") as string ?? outcome;
        activity?.SetTag("steamapp.outcome", effectiveOutcome);

        TagList tags = default;
        tags.Add("steamapp.operation", operation);
        tags.Add("steamapp.outcome", effectiveOutcome);
        BackgroundOperationCount.Add(1, tags);
        BackgroundOperationDuration.Record(
            Math.Max(0, duration.TotalSeconds),
            tags);
    }

    internal static void RecordQueueDelay(
        string queue,
        DateTime requestedAtUtc,
        DateTime observedAtUtc)
    {
        TagList tags = default;
        tags.Add("messaging.destination.name", queue);
        MessagingQueueDelay.Record(
            Math.Max(0, (observedAtUtc - requestedAtUtc).TotalSeconds),
            tags);
    }
}
