using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer.Controllers
{
    [ApiController]
    [Route("listing")]
    public class ListingController : ControllerBase
    {
        //private readonly ILogger<SteamController> _logger;
        private readonly ISteamService _steamService;

        public ListingController(ISteamService steamService)//, ILogger<SteamController> logger )
        {
            //_logger = logger;
            _steamService = steamService;
        }

        [HttpGet("listings")]
        public IActionResult GetProducts()
        {
            try
            {
                var result = _steamService.GetProductsAsync().GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }

        [HttpPost("listings")]
        public IActionResult CreateProduct(Product product)
        {
            try
            {
                var result = _steamService.CreateProductAsync(product).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }

        [HttpPut("listings/{id}")]
        public IActionResult UpdateProduct(long id, Product products)
        {
            try
            {
                var result = _steamService.UpdateProductAsync(products).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.InnerException);
            }
        }

        [HttpDelete("listings/{id}")]
        public IActionResult DeleteProduct(long id)
        {
            try
            {
                var result = _steamService.DeleteProductAsync(id).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException);
            }
        }
    }
}
