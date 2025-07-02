using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("steam")]
public class SteamController : ControllerBase
{
    // private readonly ILogger<SteamController> _logger;
    private readonly ISteamService _steamService;

    public SteamController(ISteamService steamService)//, ILogger<SteamController> logger)
    {
        //_logger = logger;
        _steamService = steamService;
    }

    /// <summary>
    /// Returns Hat Listing URLs for a batch
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/urls/fromPage/{fromPage}/batchSize/{batchSize}")]
    public IActionResult GetHatBatchUrls(short fromPage, short batchSize = 1)
    {
        try
        {
            var result = _steamService.GetHatBatchUrls(fromPage, batchSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Returns Weapon Listing URLs for a batch
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("weapon/urls/fromIndex/{fromIndex}/batchSize/{batchSize}")]
    public IActionResult GetWeaponBatchUrls(short fromIndex, short batchSize = 1)
    {
        try
        {
            var result = _steamService.GetWeaponBatchUrls(fromIndex, batchSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Web scrapes a page from the Community Market
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/page/{page}")]
    public IActionResult ScrapePageAsync(short page)
    {
        try
        {
            var result = _steamService.ScrapePageAsync(page).GetAwaiter().GetResult();
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
    public IActionResult GetFilteredBulkListings(short page)
    {
        try
        {
            var result = _steamService.GetFilteredBulkListingsAsync(page).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    /// <summary>
    /// Works.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    [HttpGet("hat/page/{page}/painted")]
    public IActionResult ScrapePageForPaintedListingsOnly(short page)
    {
        try
        {
            var result = _steamService.ScrapePageForPaintedListingsOnlyAsync(page).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Works.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("hat/name/{name}/is-painted")]
    public IActionResult CheckIsListingPainted(string name)
    {
        try
        {
            var result = _steamService.CheckIsListingPaintedAsync(name).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (JsonSerializationException ex)
        {
            return StatusCode(400, "Error: Invalid Listing");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
