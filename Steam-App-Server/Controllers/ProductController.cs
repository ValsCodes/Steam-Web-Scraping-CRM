using Microsoft.AspNetCore.Mvc;
using SteamAppServer.Models.DTO;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer.Controllers
{
    [ApiController]
    [Route("product")]
    public class ProductController : ControllerBase
    {
        //private readonly ILogger<SteamController> _logger;
        private readonly ISteamService _steamService;

        public ProductController(ISteamService steamService)//, ILogger<SteamController> logger )
        {
            //_logger = logger;
            _steamService = steamService;
        }

        [HttpGet("get")]
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
        [HttpGet("get/all")]
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

        [HttpPost("create")]
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

        [HttpPost("create/bulk")]
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

        [HttpPut("update")]
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

        [HttpPut("update/bulk")]
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


        [HttpDelete("delete")]
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

        [HttpDelete("delete/bulk")]
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
