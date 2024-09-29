using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models.Proxies;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer.Controllers
{
    [ApiController]
    [Route("steam")]
    public class SteamController : ControllerBase
    {
        private readonly ILogger<SteamController> _logger;
        private readonly ISteamService _steamService;

        public SteamController(ILogger<SteamController> logger, ISteamService steamService)
        {
            _logger = logger;
            _steamService = steamService;
        }

        [HttpGet("results/filterred/page_{page}")]
        public IEnumerable<ListingProxy> GetFilterredListings(short page)
        {
            var result = _steamService.GetFilterredListingsAsync(page).GetAwaiter().GetResult();
            if (result.Any())
            {
                return result;
            }

            return Enumerable.Empty<ListingProxy>();
        }

        [HttpGet("results/painted-only/page_{page}")]
        public IEnumerable<ListingProxy> GetPaintedListings(short page)
        {
            try
            {
                var result = _steamService.GetPaintedListingsOnlyAsync(page).GetAwaiter().GetResult();
                if (!result.Any())
                {
                    return Enumerable.Empty<ListingProxy>();
                }

                return result;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<ListingProxy>();
            }
        }

        [HttpGet("result/{name}/is_painted")]
        public (bool,string) IsListingPainted(string name)
        {
            var result = _steamService.IsListingPaintedAsync(name).GetAwaiter().GetResult();

            return result;
        }
    }
}
