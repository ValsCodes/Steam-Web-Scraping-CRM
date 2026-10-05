using System.Diagnostics;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Observability;

namespace SteamApp.WebAPI.Jobs;

public sealed class ManualCheckWorker(
    IManualCheckQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<ManualCheckWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MarkInterruptedRunsAsync(stoppingToken);

        await foreach (var workItem in queue.ReadAllAsync(stoppingToken))
        {
            const string operation = "manual-check.execute";
            var startedAt = Stopwatch.GetTimestamp();
            using var activity = SteamAppTelemetry.StartOperation(operation);
            var outcome = SteamAppTelemetry.SuccessOutcome;
            using var runCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                workItem.CancellationToken);
            try
            {
                using var scope = scopeFactory.CreateScope();
                var execution = scope.ServiceProvider.GetRequiredService<IManualCheckExecutionService>();
                await execution.ExecuteAsync(
                    workItem.RunId,
                    runCancellation.Token,
                    workItem.PauseToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                outcome = SteamAppTelemetry.CancelledOutcome;
                break;
            }
            catch (OperationCanceledException) when (workItem.CancellationToken.IsCancellationRequested)
            {
                outcome = SteamAppTelemetry.CancelledOutcome;
                logger.LogInformation(
                    "Manual check run {RunId} stopped after a user cancellation.",
                    workItem.RunId);
            }
            catch (Exception exception)
            {
                outcome = SteamAppTelemetry.ErrorOutcome;
                SteamAppTelemetry.MarkError(activity);
                logger.LogError(exception, "Manual check worker failed while processing run {RunId}.", workItem.RunId);
                await MarkRunFailedAsync(workItem.RunId, exception, stoppingToken);
            }
            finally
            {
                SteamAppTelemetry.CompleteOperation(
                    activity,
                    operation,
                    outcome,
                    Stopwatch.GetElapsedTime(startedAt));
                queue.Complete(workItem.RunId, workItem.WorkItemId);
            }
        }
    }

    private async Task MarkInterruptedRunsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dataService = scope.ServiceProvider.GetRequiredService<IManualCheckDataService>();
        await dataService.MarkInterruptedRunsFailedAsync(cancellationToken);
    }

    private async Task MarkRunFailedAsync(
        long runId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IManualCheckDataService>();
            await dataService.FailAsync(runId, exception.Message, results: null, cancellationToken);
        }
        catch (Exception markFailedException)
        {
            logger.LogError(
                markFailedException,
                "Manual check worker could not mark run {RunId} as failed.",
                runId);
        }
    }
}
