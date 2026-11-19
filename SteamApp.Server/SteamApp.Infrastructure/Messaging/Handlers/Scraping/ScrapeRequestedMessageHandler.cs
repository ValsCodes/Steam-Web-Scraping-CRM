using Microsoft.Extensions.Logging;
using SteamApp.Infrastructure.Observability;
using SteamApp.Application.Features.Scraping;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using SteamApp.Contracts.WatchItem;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Infrastructure.Caching;
using SteamApp.Application.Abstractions.Persistence;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Contracts.Events.Scraping;

namespace SteamApp.Infrastructure.Messaging.Handlers.Scraping;

public sealed class ScrapeRequestedMessageHandler(
    ILogger<ScrapeRequestedMessageHandler> logger,
    IDistributedCache cache,
    IScrapeExecutionService scrapeExecution,
    IScrapeHistoryDataService scrapeHistoryData)
{
    private static readonly TimeSpan ResultCacheTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ClaimDuration = TimeSpan.FromMinutes(5);

    public async Task HandleAsync(ScrapeRequested message, CancellationToken cancellationToken)
    {
        var job = await scrapeHistoryData.GetJobAsync(message.HistoryId, cancellationToken);
        if (job is null || string.IsNullOrWhiteSpace(job.CorrelationId) ||
            job.CorrelationId != message.CorrelationId || job.UserId != message.UserId ||
            job.GameUrlId != message.GameUrlId || job.Page != message.Page ||
            job.Endpoint != message.Endpoint || job.ScrapeType != message.ScrapeType)
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            logger.LogWarning("Ignored a scrape message that did not match its persisted job.");
            return;
        }

        await ExecuteJobAsync(job, cancellationToken);
    }

    public async Task RecoverAsync(long historyId, CancellationToken cancellationToken)
    {
        var job = await scrapeHistoryData.GetJobAsync(historyId, cancellationToken);
        if (job is not null)
        {
            await ExecuteJobAsync(job, cancellationToken);
        }
    }

    private async Task ExecuteJobAsync(ScrapeHistoryJob job, CancellationToken cancellationToken)
    {
        var claimId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        if (!await scrapeHistoryData.TryClaimJobAsync(job.Id, claimId, now, now.Add(ClaimDuration), cancellationToken))
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            return;
        }

        using var claimCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = claimCancellation.Token;
        var renewal = RenewClaimAsync(job.Id, claimId, claimCancellation);
        try
        {
            if (job.UserId is null ||
                await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(job.GameUrlId, job.UserId, token) is null)
            {
                await scrapeHistoryData.CompleteJobAsync(job.Id, claimId, null, "The game URL is unavailable.", token);
                return;
            }

            string cacheKey;
            IReadOnlyList<WatchItemDto>? cached;
            IReadOnlyList<WatchItemDto> results;
            try
            {
                cacheKey = ScrapeOperation.GetCacheKey(job.Endpoint, job.GameUrlId, job.Page);
                cached = await ReadCacheAsync(cacheKey, token);
                results = cached ?? await scrapeExecution.ExecuteAsync(job.Endpoint, job.GameUrlId, job.Page, token);
                token.ThrowIfCancellationRequested();
            }
            catch (Exception exception) when (!token.IsCancellationRequested)
            {
                SteamAppTelemetry.MarkError(Activity.Current);
                var mapped = ScrapeErrorResult.FromException(job.Endpoint, exception);
                logger.Log(mapped.LogLevel, "Scrape job {HistoryId} failed with status {StatusCode}.",
                    job.Id, mapped.StatusCode);
                await scrapeHistoryData.CompleteJobAsync(job.Id, claimId, null, mapped.Message, token);
                return;
            }

            if (await scrapeHistoryData.CompleteJobAsync(job.Id, claimId, results, null, token) && cached is null)
            {
                try
                {
                    await cache.SetJsonAsync(cacheKey, results, ResultCacheTtl, token);
                }
                catch (Exception) when (!token.IsCancellationRequested)
                {
                    logger.LogWarning("Unable to cache the result for scrape job {HistoryId}.", job.Id);
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
        }
        finally
        {
            await claimCancellation.CancelAsync();
            await renewal;
        }
    }

    private async Task<List<WatchItemDto>?> ReadCacheAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetJsonAsync<List<WatchItemDto>>(key, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Scrape result cache unavailable. Fetching fresh results.");
            return null;
        }
    }

    private async Task RenewClaimAsync(long historyId, Guid claimId, CancellationTokenSource cancellation)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellation.Token))
            {
                var now = DateTime.UtcNow;
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token);
                timeout.CancelAfter(TimeSpan.FromSeconds(30));
                if (!await scrapeHistoryData.RenewClaimAsync(historyId, claimId, now, now.Add(ClaimDuration), timeout.Token))
                {
                    await cancellation.CancelAsync();
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to renew the claim for scrape job {HistoryId}.", historyId);
            await cancellation.CancelAsync();
        }
    }
}
