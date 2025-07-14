using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
[Authorize]
public class SteamController(ISteamService steamService/*, ILogger<SteamController> logger*/) : ControllerBase
{
    /// <summary>
    /// Returns the RGB values found on a specific pixel of an item src image
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    [HttpGet("hat/paint-info-source")]
    public IActionResult GetPaintInfoFromSourceAsync(string src, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.GetPaintInfoFromSource(src, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Web scrapes content from a page of the Steam Community Market
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/page/{page}")]
    public IActionResult ScrapePageAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.ScrapePage(page, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// ScrapePageAsync() + Checks the src images of the scraped listings if they contain a pixel at a specific location 
    /// matching a desirable paint color
    /// </summary>
    /// <param name="page"></param>
    /// <param name="isGoodPaintsOnly"></param>
    /// <returns></returns>
    [HttpGet("hat/check-paint-by-pixel/{page}")]
    public IActionResult ScrapePageWithSrcPixelPaintCheckAsync(short page, bool isGoodPaintsOnly = true, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.ScrapePageWithSrcPixelPaintCheck(page, isGoodPaintsOnly, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Returns a Deserialized, filtered json result for 100 listings
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/page/{page}/bulk")]
    public IActionResult GetDeserializedLisitngsFromUrlAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.GetDeserializedLisitngsFromUrl(page, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Checks if the First listing with this name in the steam marketplace has a paint.
    /// Extracts the data from a json responce.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("hat/name/{name}/is-painted")]
    public IActionResult CheckIsListingPaintedAsync(string name, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.CheckIsListingPainted(name, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (JsonSerializationException)
        {
            return StatusCode(400, "Error: Invalid Listing");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// ScrapePageAsync + CheckIsListingPaintedAsync.
    /// Currently the most brute force way to make the check.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/page/{page}/painted")]
    public IActionResult ScrapePageForPaintedListingsOnlyAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = steamService.ScrapePageForPaintedListingsOnly(page, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
