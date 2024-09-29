using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models;
using SteamAppServer.Models.Partials;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer.Controllers
{
    [ApiController]
    [Route("sell-listings")]
    public class SellListingsController : ControllerBase
    {
        private readonly ILogger<SteamController> _logger;
        private readonly ISteamService _steamService;

        public SellListingsController(ILogger<SteamController> logger, ISteamService steamService)
        {
            _logger = logger;
            _steamService = steamService;
        }

        [HttpGet("listings")]
        public IEnumerable<SellListing> GetListings()
        {
            var result = _steamService.GetListingsAsync().GetAwaiter().GetResult();
            return result;
        }

        [HttpPost("listings")]
        public SellListing CreateListing(SellListing sellListing)
        {
            var result = _steamService.CreateListingAsync(sellListing).GetAwaiter().GetResult();
            return result;
        }

        [HttpPut("listings/{id}")]
        public SellListing UpdateListing(long id, SellListing sellListing)
        {
            var result = _steamService.UpdateListingAsync(id, sellListing).GetAwaiter().GetResult();
            return result;
        }

        [HttpPatch("listings/{id}")]
        public SellListing UpdatePartiallyListing(long id, SellListingPartial sellListing)
        {
            var result = _steamService.UpdateListingPartialAsync(id, sellListing).GetAwaiter().GetResult();
            return result;
        }

        [HttpDelete("listings/{id}")]
        public SellListing DeletelListing(long id)
        {
            var result = _steamService.DeleteListingAsync(id).GetAwaiter().GetResult();
            return result;
        }
    }
}
