using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models;
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

        [HttpGet("filterred-listings")]
        public IEnumerable<ListingProxy> GetFilterredListings(short page)
        {
            var result = _steamService.GetFilterredListingsAsync(page).GetAwaiter().GetResult();
            if (result.Any())
            {
                return result;
            }

            return Enumerable.Empty<ListingProxy>();
        }

        [HttpGet("painted-listings")]
        public IEnumerable<ListingProxy> GetPaintedListings(short page)
        {
            var result = _steamService.GetPaintedListingsOnlyAsync(page).GetAwaiter().GetResult();
            if (result.Any())
            {
                return result;
            }

            return Enumerable.Empty<ListingProxy>();
        }

        [HttpGet("listings/{name}/is-painted")]
        public (bool,string) IsListingPainted(string name)
        {
            var result = _steamService.IsListingPaintedAsync(name).GetAwaiter().GetResult();

            return result;
        }

        [HttpGet("sell-listings")]
        public IEnumerable<SellListing> GetSellListings()
        {
            var result = _steamService.GetSellListingsAsync().GetAwaiter().GetResult();
            return result;
        }

        [HttpGet("get-batch-manually")]
        public void GetPagesManually(long page, long batchSize)
        {
            _steamService.GetPagesManuallyAsync(page, batchSize).GetAwaiter().GetResult();
        }
    }
}
