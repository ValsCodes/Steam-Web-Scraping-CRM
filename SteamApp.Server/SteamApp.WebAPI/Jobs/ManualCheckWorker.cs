using SteamApp.Interfaces.Services;

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
            using var runCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                workItem.CancellationToken);
            try
            {
                using var scope = scopeFactory.CreateScope();
                var execution = scope.ServiceProvider.GetRequiredService<IManualCheckExecutionService>();
                await execution.ExecuteAsync(workItem.RunId, runCancellation.Token);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (OperationCanceledException) when (workItem.CancellationToken.IsCancellationRequested)
            {
                logger.LogInformation(
                    "Manual check run {RunId} stopped after a user cancellation.",
                    workItem.RunId);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Manual check worker failed while processing run {RunId}.", workItem.RunId);
                await MarkRunFailedAsync(workItem.RunId, exception, stoppingToken);
            }
            finally
            {
                queue.Complete(workItem.RunId);
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
