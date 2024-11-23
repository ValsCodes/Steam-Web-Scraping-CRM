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

        [HttpGet("results/filtered/page_{page}")]
        public IActionResult GetFilterredListings(short page)
        {
            try
            {
                var result = _steamService.GetFilterredListingsAsync(page).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }

        [HttpGet("results/painted-only/page_{page}")]
        public IActionResult GetPaintedListings(short page)
        {
            try
            {
                var result = _steamService.GetPaintedListingsOnlyAsync(page).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }

        [HttpGet("result/{name}/is_painted")]
        public IActionResult IsListingPainted(string name)
        {
            try
            {
                var result = _steamService.IsListingPaintedAsync(name).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }
    }
}
