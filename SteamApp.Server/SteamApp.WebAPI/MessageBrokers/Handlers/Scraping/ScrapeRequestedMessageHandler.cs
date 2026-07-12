using Microsoft.Extensions.Caching.Distributed;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Domain.Enums;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Messages.Scraping;
using SteamApp.WebAPI.Services;

namespace SteamApp.WebAPI.MessageBrokers.Handlers.Scraping;

public sealed class ScrapeRequestedMessageHandler(
    ILogger<ScrapeRequestedMessageHandler> logger,
    IDistributedCache cache,
    IScrapeExecutionService scrapeExecution,
    IScrapeHistoryDataService scrapeHistoryData)
{
    private static readonly TimeSpan ResultCacheTtl = TimeSpan.FromMinutes(5);

    public async Task HandleAsync(
        ScrapeRequested message,
        CancellationToken cancellationToken)
    {
        var job = await scrapeHistoryData.GetJobAsync(message.HistoryId, cancellationToken);
        if (job is null)
        {
            logger.LogWarning(
                "Scrape message {CorrelationId} skipped because history row {HistoryId} was not found.",
                message.CorrelationId,
                message.HistoryId);
            return;
        }

        if (job.Status is ScrapeJobStatusEnum.Succeeded or ScrapeJobStatusEnum.Failed)
        {
            logger.LogInformation(
                "Scrape message {CorrelationId} skipped because history row {HistoryId} is already {Status}.",
                message.CorrelationId,
                message.HistoryId,
                job.Status);
            return;
        }

        var runningStatus = await scrapeHistoryData.MarkRunningAsync(
            message.HistoryId,
            cancellationToken);

        if (runningStatus is ScrapeJobStatusEnum.Succeeded or ScrapeJobStatusEnum.Failed)
        {
            logger.LogInformation(
                "Scrape message {CorrelationId} skipped because history row {HistoryId} is already {Status}.",
                message.CorrelationId,
                message.HistoryId,
                runningStatus);
            return;
        }

        try
        {
            var cacheKey = ScrapeEndpointDefinitions.GetCacheKey(
                message.Endpoint,
                message.GameUrlId,
                message.Page);
            var cached = await cache.GetJsonAsync<List<WatchItemDto>>(
                cacheKey,
                cancellationToken);

            if (cached is not null)
            {
                await scrapeHistoryData.MarkSucceededAsync(
                    message.HistoryId,
                    cached,
                    cancellationToken);

                logger.LogInformation(
                    "Scrape message {CorrelationId} completed from cache for history row {HistoryId}.",
                    message.CorrelationId,
                    message.HistoryId);
                return;
            }

            var result = await scrapeExecution.ExecuteAsync(
                message.Endpoint,
                message.GameUrlId,
                message.Page);

            await cache.SetJsonAsync(
                cacheKey,
                result,
                ResultCacheTtl,
                cancellationToken);

            await scrapeHistoryData.MarkSucceededAsync(
                message.HistoryId,
                result,
                cancellationToken);

            logger.LogInformation(
                "Scrape message {CorrelationId} completed for history row {HistoryId}.",
                message.CorrelationId,
                message.HistoryId);
        }
        catch (Exception exception)
        {
            var mapped = ScrapeEndpointDefinitions.MapError(message.Endpoint, exception);

            if (mapped.LogLevel == LogLevel.Warning)
            {
                logger.LogWarning(exception, "Scrape message {CorrelationId} failed with a mapped warning.", message.CorrelationId);
            }
            else
            {
                logger.LogError(exception, "Scrape message {CorrelationId} failed.", message.CorrelationId);
            }

            await scrapeHistoryData.MarkFailedAsync(
                message.HistoryId,
                mapped.Message,
                cancellationToken);
        }
    }
}
