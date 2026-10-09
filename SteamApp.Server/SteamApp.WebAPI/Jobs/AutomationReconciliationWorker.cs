using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Jobs;

public sealed class AutomationReconciliationWorker(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IServiceScopeFactory scopeFactory,
    IManualCheckQueue manualCheckQueue,
    ILogger<AutomationReconciliationWorker> logger) : BackgroundService
{
    private sealed record RunOwner(long Id, string UserId);

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Automation reconciliation iteration failed.");
            }
        }
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var activeManualRuns = await db.ManualCheckRuns.AsNoTracking()
            .Where(x =>
                x.AutomaticQueueRunBlock == null &&
                (x.Status == ManualCheckRunStatusEnum.Queued ||
                 x.Status == ManualCheckRunStatusEnum.Running ||
                 x.Status == ManualCheckRunStatusEnum.PauseRequested))
            .Select(x => new RunOwner(x.Id, x.UserId))
            .ToListAsync(cancellationToken);
        var activeQueueRuns = await db.AutomaticQueueRuns.AsNoTracking()
            .Where(x =>
                x.Status == AutomaticQueueRunStatusEnum.Queued ||
                x.Status == AutomaticQueueRunStatusEnum.Running ||
                x.Status == AutomaticQueueRunStatusEnum.PauseRequested)
            .Select(x => new RunOwner(x.Id, x.UserId))
            .ToListAsync(cancellationToken);
        var quotaPausedManualRuns = await db.ManualCheckRuns.AsNoTracking()
            .Where(x =>
                x.AutomaticQueueRunBlock == null &&
                x.Status == ManualCheckRunStatusEnum.Paused &&
                x.PauseReason == AutomationPauseReasonEnum.QuotaExceeded)
            .Select(x => new RunOwner(x.Id, x.UserId))
            .ToListAsync(cancellationToken);
        var quotaPausedQueueRuns = await db.AutomaticQueueRuns.AsNoTracking()
            .Where(x =>
                x.Status == AutomaticQueueRunStatusEnum.Paused &&
                x.PauseReason == AutomationPauseReasonEnum.QuotaExceeded)
            .Select(x => new RunOwner(x.Id, x.UserId))
            .ToListAsync(cancellationToken);

        using var scope = scopeFactory.CreateScope();
        var access = scope.ServiceProvider.GetRequiredService<IAutomationAccessService>();
        var manualData = scope.ServiceProvider.GetRequiredService<IManualCheckDataService>();
        var queueData = scope.ServiceProvider.GetRequiredService<IAutomaticQueueDataService>();
        var userIds = activeManualRuns.Select(x => x.UserId)
            .Concat(activeQueueRuns.Select(x => x.UserId))
            .Concat(quotaPausedManualRuns.Select(x => x.UserId))
            .Concat(quotaPausedQueueRuns.Select(x => x.UserId))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        foreach (var userId in userIds)
        {
            var usage = await access.GetUsageAsync(userId, cancellationToken);
            if (usage.Unlimited)
            {
                await ResumeQuotaPausedAsync(userId, quotaPausedManualRuns, quotaPausedQueueRuns, manualData, queueData, cancellationToken);
                continue;
            }

            var reason = !usage.PresenceActive
                ? AutomationPauseReasonEnum.PresenceLost
                : usage.RemainingSeconds <= 0
                    ? AutomationPauseReasonEnum.QuotaExceeded
                    : (AutomationPauseReasonEnum?)null;
            if (reason.HasValue)
            {
                foreach (var run in activeManualRuns.Where(x => x.UserId == userId))
                {
                    try
                    {
                        await manualData.PauseAsync(run.Id, cancellationToken, reason.Value);
                        manualCheckQueue.TryPause(run.Id);
                    }
                    catch (ManualCheckRequestException)
                    {
                    }
                }
                foreach (var run in activeQueueRuns.Where(x => x.UserId == userId))
                {
                    try
                    {
                        await queueData.PauseRunAsync(run.Id, userId, cancellationToken, reason.Value);
                    }
                    catch (AutomaticQueueRequestException)
                    {
                    }
                }
                continue;
            }

            await ResumeQuotaPausedAsync(userId, quotaPausedManualRuns, quotaPausedQueueRuns, manualData, queueData, cancellationToken);
        }
    }

    private async Task ResumeQuotaPausedAsync(
        string userId,
        IEnumerable<RunOwner> manualRuns,
        IEnumerable<RunOwner> queueRuns,
        IManualCheckDataService manualData,
        IAutomaticQueueDataService queueData,
        CancellationToken cancellationToken)
    {
        foreach (var run in manualRuns.Where(x => x.UserId == userId))
        {
            try
            {
                var resumed = await manualData.ContinueAsync(run.Id, cancellationToken);
                await manualCheckQueue.EnqueueAsync(resumed.Id, cancellationToken);
            }
            catch (ManualCheckRequestException)
            {
            }
        }
        foreach (var run in queueRuns.Where(x => x.UserId == userId))
        {
            try
            {
                await queueData.ContinueRunAsync(run.Id, userId, cancellationToken);
            }
            catch (AutomaticQueueRequestException)
            {
            }
        }
    }
}
