using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Messages.Scraping;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Scraping;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
[Authorize(Policy = SecurityPolicies.ApiUser)]
[EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
public class SteamController(
    IWishlistService wishlistService,
    ILogger<SteamController> logger,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IScrapeHistoryDataService scrapeHistoryData,
    IScrapeExecutionService scrapeExecution,
    IMessagePublisher messagePublisher,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IMemoryCache cache) : ControllerBase
{
    private readonly RabbitMqOptions rabbitMq = rabbitMqOptions.Value;

    [HttpGet("scrape-page/gameUrl/{gamerUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapePageAsync(long gamerUrlId, short page)
    {

        return await RunScrapeEndpointAsync(
            gamerUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePageEndpoint,
            ScrapeEndpointDefinitions.WebScrapeType,
            validatePage: true);
    }

    [HttpGet("scrape-public-api/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        return await RunScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePublicApiEndpoint,
            ScrapeEndpointDefinitions.PublicApiScrapeType);
    }

    [HttpGet("scrape-pixels/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeForPixelsAsync(long gameUrlId, short page)
    {
        return await  RunScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePixelsEndpoint,
            ScrapeEndpointDefinitions.PixelScrapeType);
    }

    [HttpPost("scrape-jobs/scrape-page/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> QueueScrapePageAsync(
        long gameUrlId,
        short page,
        CancellationToken cancellationToken = default)
    {
        return await QueueScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePageEndpoint,
            ScrapeEndpointDefinitions.WebScrapeType,
            validatePage: true,
            cancellationToken: cancellationToken);
    }

    [HttpPost("scrape-jobs/scrape-public-api/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> QueueScrapeFromPublicApiAsync(
        long gameUrlId,
        short page,
        CancellationToken cancellationToken = default)
    {
        return await QueueScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePublicApiEndpoint,
            ScrapeEndpointDefinitions.PublicApiScrapeType,
            validatePage: false,
            cancellationToken: cancellationToken);
    }

    [HttpPost("scrape-jobs/scrape-pixels/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> QueueScrapeForPixelsAsync(
        long gameUrlId,
        short page,
        CancellationToken cancellationToken = default)
    {
        return await QueueScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapeEndpointDefinitions.ScrapePixelsEndpoint,
            ScrapeEndpointDefinitions.PixelScrapeType,
            validatePage: false,
            cancellationToken: cancellationToken);
    }

    [HttpGet("scrape-history")]
    public async Task<IActionResult> GetScrapeHistoryAsync(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        take = Math.Clamp(take, 1, 500);

        var history = await scrapeHistoryData.GetHistoryAsync(userId, take, cancellationToken);
        return Ok(history);
    }

    [HttpGet("scrape-history/{id:long}")]
    public async Task<IActionResult> GetScrapeHistoryDetailAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var detail = await scrapeHistoryData.GetDetailAsync(id, userId, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return Ok(detail);
    }

    [HttpPost("scrape-history/{id:long}/rerun")]
    public async Task<IActionResult> RerunScrapeHistoryAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var original = await scrapeHistoryData.GetRerunSourceAsync(id, userId, cancellationToken);
        if (original is null)
        {
            return NotFound();
        }

        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            original.GameUrlId,
            userId,
            cancellationToken);
        if (gameUrl is null)
        {
            return NotFound("Original Game URL no longer exists.");
        }

        try
        {
            var result = await ExecuteScrapeServiceAsync(
                original.Endpoint,
                original.GameUrlId,
                original.Page);

            cache.Set(
                ScrapeEndpointDefinitions.GetCacheKey(original.Endpoint, original.GameUrlId, original.Page),
                result,
                TimeSpan.FromMinutes(5));

            var newRecord = await scrapeHistoryData.AddHistoryAsync(
                userId,
                gameUrl,
                original.Page,
                original.Endpoint,
                original.ScrapeType,
                result,
                errorText: null,
                cancellationToken);

            return Ok(new ScrapeHistoryRerunResponseDto
            {
                History = ToSummaryDto(newRecord, gameUrl.GameUrlName),
                Results = result
            });
        }
        catch (Exception ex)
        {
            var mapped = ScrapeEndpointDefinitions.MapError(original.Endpoint, ex);

            LogScrapeException(ex, mapped);

            var newRecord = await scrapeHistoryData.AddHistoryAsync(
                userId,
                gameUrl,
                original.Page,
                original.Endpoint,
                original.ScrapeType,
                results: null,
                errorText: mapped.Message,
                cancellationToken);

            return StatusCode(mapped.StatusCode, new ScrapeHistoryRerunResponseDto
            {
                History = ToSummaryDto(newRecord, gameUrl.GameUrlName),
                Results = [],
                ErrorText = mapped.Message
            });
        }
    }

    [HttpPost("scrape-history/{id:long}/rerun-async")]
    public async Task<IActionResult> RerunScrapeHistoryAsyncQueued(
        long id,
        CancellationToken cancellationToken = default)
    {
        if (!rabbitMq.Enabled)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "RabbitMQ is not enabled.");
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var original = await scrapeHistoryData.GetRerunSourceAsync(id, userId, cancellationToken);
        if (original is null)
        {
            return NotFound();
        }

        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            original.GameUrlId,
            userId,
            cancellationToken);
        if (gameUrl is null)
        {
            return NotFound("Original Game URL no longer exists.");
        }

        return await QueueScrapeForGameUrlAsync(
            userId,
            gameUrl,
            original.Page,
            original.Endpoint,
            original.ScrapeType,
            cancellationToken);
    }

    [HttpGet("check-wishlist/{wishlistId}")]
    public async Task<IActionResult> CheckWithlistItem(long wishlistId)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(CheckWithlistItem)))
        {
            try
            {
                if (!await CurrentUserOwnsWishlistItemAsync(wishlistId))
                {
                    return NotFound();
                }

                var cacheKey = string.Format(CacheKeys.WishListItem, wishlistId);

                if (cache.TryGetValue(cacheKey, out var cached))
                {
                    return Ok(cached);
                }

                var result = await wishlistService.CheckWishlistItem(wishlistId);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return Ok(result);
            }
            catch (JsonSerializationException ex)
            {
                logger.LogWarning(ex, "Invalid listing.");
                return StatusCode(400, "Error: Invalid Wishlisting");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    #region Not Done
    //[HttpGet("pixel-info/gameUrl/{gameUrlId}")]
    //public async Task<IActionResult> GetPixelInfoFromSourceAsync(long gameUrlId, string srcUrl)
    //{
    //    using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(GetPixelInfoFromSourceAsync)))
    //    {
    //        try
    //        {
    //            if (gameUrlId <= 0 || string.IsNullOrWhiteSpace(srcUrl))
    //            {
    //                return BadRequest("Invalid parameters.");
    //            }

    //            var cacheKey = string.Format(CacheKeys.PixelInfo, gameUrlId, srcUrl);

    //            if (cache.TryGetValue(cacheKey, out object cached))
    //            {
    //                return Ok(cached);
    //            }

    //            var result = await steamService.GetPixelInfoFromSource(gameUrlId, srcUrl);

    //            cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
    //            return Ok(result);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.LogError(ex, "Request failed.");
    //            return StatusCode(500, ex.Message);
    //        }
    //    }
    //}

    /*[HttpGet("scrape-product-page/{gameId}/pixels")]
    public async Task<IActionResult> ScrapeProductForPixelsAsync(long gameId, string productName)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeProductForPixelsAsync)))
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ProductPixels, gameId, productName);

                if (cache.TryGetValue(cacheKey, out object cached))
                {
                    return Ok(cached);
                }

                var result = await steamService.ScrapeProductPixels(gameId, productName);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }*/
    #endregion

    private async Task<IActionResult> RunScrapeEndpointAsync(
        long gameUrlId,
        short page,
        string endpoint,
        string scrapeType,
        bool validatePage = false)
    {
        using (logger.BeginScope("{Controller}.{Endpoint}", nameof(SteamController), endpoint))
        {
            var userId = User.GetUserId();
            OwnedGameUrlSnapshot? gameUrl = null;

            try
            {
                if (validatePage && (page < 0 || page > short.MaxValue))
                {
                    throw new ArgumentOutOfRangeException(nameof(page));
                }

                if (userId is null)
                {
                    return Unauthorized();
                }

                gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
                    gameUrlId,
                    userId,
                    CancellationToken.None);
                if (gameUrl is null)
                {
                    return NotFound();
                }

                var cacheKey = ScrapeEndpointDefinitions.GetCacheKey(endpoint, gameUrlId, page);

                if (cache.TryGetValue(cacheKey, out object? cached))
                {
                    await TryAddScrapeHistoryAsync(userId, gameUrl, page, endpoint, scrapeType, cached, errorText: null);
                    return Ok(cached);
                }

                var result = await scrapeExecution.ExecuteAsync(endpoint, gameUrlId, page);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                await TryAddScrapeHistoryAsync(userId, gameUrl, page, endpoint, scrapeType, result, errorText: null);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var mapped = ScrapeEndpointDefinitions.MapError(endpoint, ex);
                LogScrapeException(ex, mapped);

                if (userId is not null && gameUrl is not null)
                {
                    await TryAddScrapeHistoryAsync(
                        userId,
                        gameUrl,
                        page,
                        endpoint,
                        scrapeType,
                        results: null,
                        errorText: mapped.Message);
                }

                return StatusCode(mapped.StatusCode, mapped.Message);
            }
        }
    }

    private async Task<IActionResult> QueueScrapeEndpointAsync(
        long gameUrlId,
        short page,
        string endpoint,
        string scrapeType,
        bool validatePage,
        CancellationToken cancellationToken)
    {
        if (!rabbitMq.Enabled)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "RabbitMQ is not enabled.");
        }

        if (validatePage && page < 0)
        {
            return BadRequest("Invalid page.");
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            gameUrlId,
            userId,
            cancellationToken);
        if (gameUrl is null)
        {
            return NotFound();
        }

        return await QueueScrapeForGameUrlAsync(
            userId,
            gameUrl,
            page,
            endpoint,
            scrapeType,
            cancellationToken);
    }

    private async Task<IActionResult> QueueScrapeForGameUrlAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var history = await scrapeHistoryData.CreateQueuedHistoryAsync(
            userId,
            gameUrl,
            page,
            endpoint,
            scrapeType,
            correlationId,
            cancellationToken);

        try
        {
            await messagePublisher.PublishAsync(
                rabbitMq.ScrapeRequestQueueName,
                new ScrapeRequested(
                    history.Id,
                    userId,
                    gameUrl.GameUrlId,
                    page,
                    endpoint,
                    scrapeType,
                    history.Date,
                    correlationId),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to publish scrape job {CorrelationId} for history row {HistoryId}.",
                correlationId,
                history.Id);

            await scrapeHistoryData.MarkFailedAsync(
                history.Id,
                "Failed to queue scrape job.",
                cancellationToken);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Failed to queue scrape job.");
        }

        return BuildAcceptedScrapeJobResult(history, gameUrl.GameUrlName);
    }

    private IActionResult BuildAcceptedScrapeJobResult(
        AutomatedScrapeHistory history,
        string? gameUrlName)
    {
        var fallbackLocation = $"/steam/scrape-history/{history.Id}";
        var location = Url?.Action(nameof(GetScrapeHistoryDetailAsync), new { id = history.Id })
                       ?? fallbackLocation;

        return Accepted(location, new ScrapeJobAcceptedDto
        {
            HistoryId = history.Id,
            History = ToSummaryDto(history, gameUrlName),
            Status = history.Status,
            CorrelationId = history.CorrelationId ?? string.Empty
        });
    }

    private async Task<IReadOnlyList<SteamApp.Application.DTOs.WatchItem.WatchItemDto>> ExecuteScrapeServiceAsync(
        string endpoint,
        long gameUrlId,
        short page)
    {
        return await scrapeExecution.ExecuteAsync(endpoint, gameUrlId, page);
    }

    private async Task<AutomatedScrapeHistory?> TryAddScrapeHistoryAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        object? results,
        string? errorText)
    {
        try
        {
            return await scrapeHistoryData.AddHistoryAsync(
                userId,
                gameUrl,
                page,
                endpoint,
                scrapeType,
                results,
                errorText,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record automated scrape history.");
            return null;
        }
    }

    private async Task<bool> CurrentUserOwnsWishlistItemAsync(long wishlistId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return false;
        }

        await using var db = dbContextFactory.CreateDbContext();

        return userId is not null &&
               await db.WishLists
                   .AsNoTracking()
                   .AnyAsync(x => x.Id == wishlistId && x.UserId == userId);
    }

    private static ScrapeHistorySummaryDto ToSummaryDto(AutomatedScrapeHistory history, string? gameUrlName)
    {
        return new ScrapeHistorySummaryDto
        {
            Id = history.Id,
            Endpoint = history.Endpoint,
            ScrapeType = history.ScrapeType,
            GameUrlId = history.GameUrlId,
            GameUrlName = gameUrlName,
            Page = history.Page,
            ResultCount = history.ResultCount,
            Date = history.Date,
            IsHaveError = history.IsHaveError,
            Status = history.Status,
            StartedAtUtc = history.StartedAtUtc,
            CompletedAtUtc = history.CompletedAtUtc,
            CorrelationId = history.CorrelationId
        };
    }

    private void LogScrapeException(Exception ex, ScrapeErrorResult mapped)
    {
        if (mapped.LogLevel == LogLevel.Warning)
        {
            logger.LogWarning(ex, "Invalid listing.");
            return;
        }

        logger.LogError(ex, "Request failed.");
    }

}
