using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure.Services;

namespace SteamApp.Controllers
{
    [ApiController]
    [Route("item")]
    public class ItemController : ControllerBase
    {
        //private readonly ILogger<SteamController> _logger;
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)//, ILogger<SteamController> logger )
        {
            //_logger = logger;
            _itemService = itemService;
        }

        //Todo itroduce filters
        [HttpGet("get/all")]
        public IActionResult GetItems()
        {
            try
            {
                var result = _itemService.GetItemsAsync().GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
