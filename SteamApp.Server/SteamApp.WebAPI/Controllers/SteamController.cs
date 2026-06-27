using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Application.Services;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
[Authorize(Policy = SecurityPolicies.ApiUser)]
[EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
public class SteamController(
    ISteamService steamService,
    IWishlistService wishlistService,
    ILogger<SteamController> logger,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IScrapeHistoryDataService scrapeHistoryData,
    IMemoryCache cache) : ControllerBase
{
    private const string ScrapePageEndpoint = "scrape-page";
    private const string ScrapePublicApiEndpoint = "scrape-public-api";
    private const string ScrapePixelsEndpoint = "scrape-pixels";

    private const string WebScrapeType = "Web Scrape";
    private const string PublicApiScrapeType = "Public API Scrape";
    private const string PixelScrapeType = "Pixel Scrape";

    [HttpGet("scrape-page/gameUrl/{gamerUrlId}/page/{page}")]
    public Task<IActionResult> ScrapePageAsync(long gamerUrlId, short page)
    {
        return RunScrapeEndpointAsync(
            gamerUrlId,
            page,
            ScrapePageEndpoint,
            WebScrapeType,
            CacheKeys.ScrapePage,
            () => steamService.ScrapePage(gamerUrlId, page),
            MapUnexpectedError,
            validatePage: true);
    }

    [HttpGet("scrape-public-api/gameUrl/{gameUrlId}/page/{page}")]
    public Task<IActionResult> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        return RunScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapePublicApiEndpoint,
            PublicApiScrapeType,
            CacheKeys.ScrapePublic,
            () => steamService.ScrapeFromPublicApi(gameUrlId, page),
            MapUnexpectedError);
    }

    [HttpGet("scrape-pixels/gameUrl/{gameUrlId}/page/{page}")]
    public Task<IActionResult> ScrapeForPixelsAsync(long gameUrlId, short page)
    {
        return RunScrapeEndpointAsync(
            gameUrlId,
            page,
            ScrapePixelsEndpoint,
            PixelScrapeType,
            CacheKeys.ScrapePixels,
            () => steamService.ScrapeWithPixels(gameUrlId, page),
            MapPixelScrapeError);
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
                GetCacheKey(original.Endpoint, original.GameUrlId, original.Page),
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
            var mapped = original.Endpoint == ScrapePixelsEndpoint
                ? MapPixelScrapeError(ex)
                : MapUnexpectedError(ex);

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
        string cacheKeyFormat,
        Func<Task<IEnumerable<WatchItemDto>>> scrape,
        Func<Exception, ScrapeErrorResult> mapError,
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

                var cacheKey = string.Format(cacheKeyFormat, gameUrlId, page);

                if (cache.TryGetValue(cacheKey, out object? cached))
                {
                    await TryAddScrapeHistoryAsync(userId, gameUrl, page, endpoint, scrapeType, cached, errorText: null);
                    return Ok(cached);
                }

                var result = MaterializeResultsIfNeeded(await scrape());

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                await TryAddScrapeHistoryAsync(userId, gameUrl, page, endpoint, scrapeType, result, errorText: null);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var mapped = mapError(ex);
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

    private async Task<IEnumerable<WatchItemDto>> ExecuteScrapeServiceAsync(
        string endpoint,
        long gameUrlId,
        short page)
    {
        var results = endpoint switch
        {
            ScrapePageEndpoint => await steamService.ScrapePage(gameUrlId, page),
            ScrapePublicApiEndpoint => await steamService.ScrapeFromPublicApi(gameUrlId, page),
            ScrapePixelsEndpoint => await steamService.ScrapeWithPixels(gameUrlId, page),
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };

        return MaterializeResultsIfNeeded(results);
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

    private static string GetCacheKey(string endpoint, long gameUrlId, short page)
    {
        return endpoint switch
        {
            ScrapePageEndpoint => string.Format(CacheKeys.ScrapePage, gameUrlId, page),
            ScrapePublicApiEndpoint => string.Format(CacheKeys.ScrapePublic, gameUrlId, page),
            ScrapePixelsEndpoint => string.Format(CacheKeys.ScrapePixels, gameUrlId, page),
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };
    }

    private static IEnumerable<WatchItemDto> MaterializeResultsIfNeeded(IEnumerable<WatchItemDto> results)
    {
        return results is ICollection<WatchItemDto> or IReadOnlyCollection<WatchItemDto>
            ? results
            : results.ToList();
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
            IsHaveError = history.IsHaveError
        };
    }

    private static ScrapeErrorResult MapPixelScrapeError(Exception ex)
    {
        return ex is JsonSerializationException
            ? new ScrapeErrorResult(StatusCodes.Status400BadRequest, "Error: Invalid Listing", LogLevel.Warning)
            : MapUnexpectedError(ex);
    }

    private static ScrapeErrorResult MapUnexpectedError(Exception ex)
    {
        return new ScrapeErrorResult(
            StatusCodes.Status500InternalServerError,
            ex.Message,
            LogLevel.Error);
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

    private sealed record ScrapeErrorResult(
        int StatusCode,
        string Message,
        LogLevel LogLevel);
}
