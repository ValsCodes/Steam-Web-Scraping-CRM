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
    public async Task<IActionResult> GetPaintInfoFromSourceAsync(string src, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.GetPaintInfoFromSource(src, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
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
    public async Task<IActionResult> ScrapePageAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.ScrapePage(page, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
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
    public async Task<IActionResult> ScrapePageWithSrcPixelPaintCheckAsync(short page, bool isGoodPaintsOnly = true, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.ScrapePageWithSrcPixelPaintCheck(page, isGoodPaintsOnly, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
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
    public async Task<IActionResult> GetDeserializedLisitngsFromUrlAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.GetDeserializedLisitngsFromUrl(page, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
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
    public async Task<IActionResult> CheckIsListingPaintedAsync(string name, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.CheckIsListingPainted(name, ct);
            return Ok(result);
        }
        catch (JsonSerializationException)
        {
            return StatusCode(400, "Error: Invalid Listing");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
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
    public async Task<IActionResult> ScrapePageForPaintedListingsOnlyAsync(short page, CancellationToken ct = default)
    {
        try
        {
            var result = await steamService.ScrapePageForPaintedListingsOnly(page, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request was canceled.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}