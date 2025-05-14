using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.DTOs.Product;
using SteamApp.Infrastructure.Services;

namespace SteamApp.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductDto>> GetById(long id, CancellationToken ct)
        {
            var dto = await _productService.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken ct)
        {
            var list = await _productService.GetListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<CreateProductResult>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var result = await _productService.CreateAsync(dto, ct);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<CreateProductResult>>> CreateBatch([FromBody] IEnumerable<CreateProductDto> dtos, CancellationToken ct)
        {
            var results = await _productService.CreateRangeAsync(dtos, ct);
            return Ok(results);
        }

        [HttpPatch]
        public async Task<ActionResult<OperationResult>> Update(long id, [FromBody] JsonPatchDocument<ProductForPatchDto> patchDoc, CancellationToken ct)
        {
            var result = await _productService.UpdateAsync(id, patchDoc, ct);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPatch("batch")]
        public async Task<ActionResult<IEnumerable<OperationResult>>> UpdateBatch([FromBody] IEnumerable<UpdateProductDto> dtos, CancellationToken ct)
        {
            var results = await _productService.UpdateRangeAsync(dtos, ct);
            return Ok(results);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<OperationResult>> Delete(long id, CancellationToken ct)
        {
            var result = await _productService.DeleteAsync(id, ct);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }

        [HttpDelete("batch")]
        public async Task<ActionResult<IEnumerable<OperationResult>>> DeleteBatch([FromBody] IEnumerable<long> ids, CancellationToken ct)
        {
            var results = await _productService.DeleteRangeAsync(ids, ct);
            return Ok(results);
        }
    }
}
