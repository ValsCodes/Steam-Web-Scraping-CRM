using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer.Controllers
{
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

        [HttpPost("get/weapon-listing-urls")]
        public IActionResult GetWeaponListingsUrls(short fromIndex, short batchSize)
        {
            try
            {
                var result = _steamService.GetWeaponListingsUrls(fromIndex, batchSize).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("get/hat-listing-urls")]
        public IActionResult GetHatListingsUrls(short fromPage, short batchSize)
        {
            try
            {
                var result = _steamService.GetHatListingsUrls(fromPage, batchSize).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("get/hat-listings")]
        public IActionResult GetFilterredListings(short page)
        {
            try
            {
                var result = _steamService.GetFilterredListingsAsync(page).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("get/hat-listings/painted")]
        public IActionResult GetPaintedListings(short page)
        {
            try
            {
                var result = _steamService.GetPaintedListingsOnlyAsync(page).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("get/is-hat-painted")]
        public IActionResult IsListingPainted(string name)
        {
            try
            {
                var result = _steamService.IsListingPaintedAsync(name).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
