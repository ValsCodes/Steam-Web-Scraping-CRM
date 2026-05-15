using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.Services;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
[Authorize(Policy = SecurityPolicies.ApiUser)]
[EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
public class SteamController(
    ISteamService steamService,
    IWishlistService wishlistService,
    ILogger<SteamController> logger,
    ApplicationDbContext db,
    IMemoryCache cache) : ControllerBase
{
    [HttpGet("scrape-page/gameUrl/{gamerUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapePageAsync(long gamerUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapePageAsync)))
        {
            try
            {
                if (page < 0 || page > short.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(page));
                }

                if (!await CurrentUserOwnsGameUrlAsync(gamerUrlId))
                {
                    return NotFound();
                }

                var cacheKey = string.Format(CacheKeys.ScrapePage, gamerUrlId, page);

                if (cache.TryGetValue(cacheKey, out object? cached))
                {
                    return Ok(cached);
                }

                var result = await steamService.ScrapePage(gamerUrlId, page);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    [HttpGet("scrape-public-api/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeFromPublicApi)))
        {
            try
            {
                if (!await CurrentUserOwnsGameUrlAsync(gameUrlId))
                {
                    return NotFound();
                }

                var cacheKey = string.Format(CacheKeys.ScrapePublic, gameUrlId, page);

                if (cache.TryGetValue(cacheKey, out object? cached))
                {
                    return Ok(cached);
                }

                var result = await steamService.ScrapeFromPublicApi(gameUrlId, page);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    [HttpGet("scrape-pixels/gameUrl/{gameUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeForPixelsAsync(long gameUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeForPixelsAsync)))
        {
            try
            {
                if (!await CurrentUserOwnsGameUrlAsync(gameUrlId))
                {
                    return NotFound();
                }

                var cacheKey = string.Format(CacheKeys.ScrapePixels, gameUrlId, page);

                if (cache.TryGetValue(cacheKey, out object? cached))
                {
                    return Ok(cached);
                }

                var result = await steamService.ScrapeWithPixels(gameUrlId, page);

                cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return Ok(result);
            }
            catch (JsonSerializationException ex)
            {
                logger.LogWarning(ex, "Invalid listing.");
                return StatusCode(400, "Error: Invalid Listing");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
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

    private async Task<bool> CurrentUserOwnsGameUrlAsync(long gameUrlId)
    {
        var userId = User.GetUserId();
        return userId is not null &&
               await db.GameUrls
                   .AsNoTracking()
                   .AnyAsync(x => x.Id == gameUrlId && x.UserId == userId);
    }

    private async Task<bool> CurrentUserOwnsWishlistItemAsync(long wishlistId)
    {
        var userId = User.GetUserId();
        return userId is not null &&
               await db.WishLists
                   .AsNoTracking()
                   .AnyAsync(x => x.Id == wishlistId && x.UserId == userId);
    }
}
