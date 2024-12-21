using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models.DTO;
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

        [HttpGet("product/get")]
        public IActionResult GetProduct([FromQuery] long? id)
        {
            try
            {
                var result = _steamService.GetProductAsync(id).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Todo itroduce filters
        [HttpGet("products/get")]
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

        [HttpPost("product/create")]
        public IActionResult CreateProduct([FromBody] ProductDto productDto)
        {
            try
            {
                var result = _steamService.CreateProductAsync(productDto).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("products/create")]
        public IActionResult CreateProducts([FromBody] ProductDto[] productDtos)
        {

            try
            {
                var result = _steamService.CreateProductsAsync(productDtos).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("product/update")]
        public IActionResult UpdateProduct([FromQuery] ProductDto productDto)
        {

            try
            {
                var result = _steamService.UpdateProductAsync(productDto).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("products/update")]
        public IActionResult UpdateProducts([FromBody] ProductDto[] productDtos)
        {
            try
            {
                var result = _steamService.UpdateProductsAsync(productDtos).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpPatch("product/update/patch")]
        //public IActionResult PatchProduct([FromQuery] ProductDto[] productDtos)
        //{
        //    try
        //    {
        //        var result = _steamService.UpdateProductsAsync(productDtos).GetAwaiter().GetResult();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpPatch("products/update/patch")]
        //public IActionResult PatchProducts([FromQuery] ProductDto[] product)
        //{
        //    throw new NotImplementedException();
        //    try
        //    {
        //        var result = _steamService.UpdateProductsAsync(request).GetAwaiter().GetResult();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.InnerException);
        //    }
        //}


        [HttpDelete("product/delete")]
        public IActionResult DeleteProduct(long? id)
        {
            try
            {
                var result = _steamService.DeleteProductAsync(id).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("products/delete")]
        public IActionResult DeleteProducts([FromQuery] long[] ids)
        {
            try
            {
                var result = _steamService.DeleteProductsAsync(ids).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
