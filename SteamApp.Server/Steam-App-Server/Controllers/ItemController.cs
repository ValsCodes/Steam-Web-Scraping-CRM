using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.DTOs.Item;
using SteamApp.Infrastructure.Services;

namespace SteamApp.Controllers
{
    [ApiController]
    [Route("item")]
    public class ItemController(IItemService itemService) : ControllerBase
    {
        //private readonly ILogger<SteamController> _logger;

        [HttpGet("{id:long}")]
        public IActionResult GetItemById(long id, CancellationToken ct)
        {
            try
            {
                var result = itemService.GetItemByIdAsync(id, ct).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetItems(CancellationToken ct, [FromQuery] string? name = null, [FromQuery] IEnumerable<long>? classFilters = null, [FromQuery] IEnumerable<long>? slotFilters = null)
        {
            try
            {
                var result = itemService.GetItemsAsync(ct, name, classFilters, slotFilters).GetAwaiter().GetResult();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult>> Create([FromBody] CreateItemDto dto, CancellationToken ct)
        {
            try
            {
                var result = await itemService.CreateItemAsync(dto, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        public async Task<ActionResult<OperationResult>> Update([FromBody] UpdateItemDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null)
                    return BadRequest("No data provided.");

                var item = await itemService.GetItemByIdAsync(dto.Id, ct);
                if (item == null)
                    return NotFound();

                if (dto.Name != null) item.Name = dto.Name;
                if (dto.IsWeapon.HasValue) item.IsWeapon = dto.IsWeapon.Value;
                if (dto.ClassId.HasValue) item.ClassId = dto.ClassId.Value;
                if (dto.SlotId.HasValue) item.SlotId = dto.SlotId.Value;
                if (dto.IsActive.HasValue) item.IsActive = dto.IsActive.Value;


                var result = await itemService.UpdateItemAsync(item, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        [HttpDelete("{id:long}")]
        public async Task<ActionResult<OperationResult>> Delete(long id, CancellationToken ct)
        {
            try
            {
                var result = await itemService.DeleteItemAsync(id, ct);
                if (!result.Success)
                    return NotFound(result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
