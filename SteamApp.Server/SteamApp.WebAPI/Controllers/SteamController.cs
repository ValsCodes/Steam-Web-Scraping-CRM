using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
[Authorize]
public class SteamController(ISteamService steamService, ILogger<SteamController> logger) : ControllerBase
{
    [HttpGet("scrape-page/gameUrl/{gamerUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapePageAsync(long gamerUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapePageAsync)))
        {
            logger.LogInformation("Request started. GamerUrlId={GamerUrlId}, Page={Page}", gamerUrlId, page);

            try
            {
                if (page < 0 || page > short.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(page), "Page is out of range.");
                }

                var result = await steamService.ScrapePage(gamerUrlId, page);

                logger.LogInformation("Request completed successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    [HttpGet("scrape-public-api/gameUrl/{gamerUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeFromPublicApi)))
        {
            logger.LogInformation("Request started. GameUrlId={GameUrlId}, Page={Page}", gameUrlId, page);

            try
            {
                var result = await steamService.ScrapeFromPublicApi(gameUrlId, page);

                logger.LogInformation("Request completed successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    [HttpGet("pixel-info/gameUrl/{gameUrlId}")]
    public async Task<IActionResult> GetPixelInfoFromSourceAsync(long gameUrlId, string srcUrl)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(GetPixelInfoFromSourceAsync)))
        {
            logger.LogInformation("Request started. GameUrlId={GameUrlId}, SrcUrl={SrcUrl}", gameUrlId, srcUrl);

            try
            {
                if (gameUrlId <= 0 || string.IsNullOrEmpty(srcUrl))
                {
                    logger.LogWarning("Validation failed.");
                    return BadRequest("Invalid parameters.");
                }

                var result = await steamService.GetPixelInfoFromSource(gameUrlId, srcUrl);

                logger.LogInformation("Request completed successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }

    [HttpGet("scrape-pixels/gameUrl/{gamerUrlId}/page/{page}")]
    public async Task<IActionResult> ScrapeForPixelsAsync(long gameUrlId, short page)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeForPixelsAsync)))
        {
            logger.LogInformation("Request started. GameUrlId={GameUrlId}, Page={Page}", gameUrlId, page);

            try
            {
                var result = await steamService.ScrapeForPixels(gameUrlId, page);

                logger.LogInformation("Request completed successfully.");
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

    [HttpGet("scrape-product-page/{gameId}/pixels")]
    public async Task<IActionResult> ScrapeProductPixelsAsync(long gameId, string productName)
    {
        using (logger.BeginScope("{Controller}.{Action}", nameof(SteamController), nameof(ScrapeProductPixelsAsync)))
        {
            logger.LogInformation("Request started. GameId={GameId}, ProductName={ProductName}", gameId, productName);

            try
            {
                var result = await steamService.ScrapeProductPixels(gameId, productName);

                logger.LogInformation("Request completed successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed.");
                return StatusCode(500, ex.Message);
            }
        }
    }
}

