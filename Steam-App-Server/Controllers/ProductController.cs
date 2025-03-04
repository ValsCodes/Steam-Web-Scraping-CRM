using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.Dto;

namespace SteamApp.Controllers
{
    [ApiController]
    [Route("product")]
    public class ProductController : ControllerBase
    {
        //private readonly ILogger<SteamController> _logger;
        private readonly IProductService _productService;

        public ProductController(IProductService productService)//, ILogger<SteamController> logger )
        {
            //_logger = logger;
            _productService = productService;
        }

        [HttpGet("get/{id}")]
        public IActionResult GetProduct(long id)
        {
            try
            {
                var result = _productService.GetProductAsync(id).GetAwaiter().GetResult();
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
                var result = _productService.GetProductsAsync().GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
        public IActionResult CreateProduct([FromBody] ProductDto productDto)
        {
            try
            {
                var result = _productService.CreateProductAsync(productDto).GetAwaiter().GetResult();
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
                var result = _productService.CreateProductsAsync(productDtos).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update")]
        public IActionResult UpdateProduct([FromBody] ProductDto productDto)
        {
            try
            {
                var result = _productService.UpdateProductAsync(productDto).GetAwaiter().GetResult();
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
                var result = _productService.UpdateProductsAsync(productDtos).GetAwaiter().GetResult();
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


        [HttpDelete("delete/{id}")]
        public IActionResult DeleteProduct(long? id)
        {
            try
            {
                var result = _productService.DeleteProductAsync(id).GetAwaiter().GetResult();
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
                var result = _productService.DeleteProductsAsync(ids).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
