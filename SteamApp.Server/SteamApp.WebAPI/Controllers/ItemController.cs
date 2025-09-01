using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs.Item;
using SteamApp.Models.OperationResults;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]

//ILogger<SteamController> _logger;
public class ItemController(IItemService itemService) : ControllerBase
{
    [HttpGet("{id:long}")]
    public IActionResult GetItemById(long id, CancellationToken ct)
    {
        try
        {
            var result = itemService.GetItemById(id, ct).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public IActionResult GetItemsAsync(CancellationToken ct, [FromQuery] string? name = null, [FromQuery] IEnumerable<long>? classFilters = null, [FromQuery] IEnumerable<long>? slotFilters = null)
    {
        try
        {
            var result = itemService.GetItems(ct, name, classFilters, slotFilters).GetAwaiter().GetResult();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<BaseOperationResult>> CreateAsync([FromBody] CreateItemDto dto, CancellationToken ct)
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
    public async Task<ActionResult<BaseOperationResult>> UpdateAsync([FromBody] UpdateItemDto dto, CancellationToken ct)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest("No data provided.");
            }

            var item = await itemService.GetItemById(dto.Id, ct);
            if (item == null)
            {
                return NotFound();
            }

            if (dto.Name != null) item.Name = dto.Name;
            if (dto.IsWeapon.HasValue) item.IsWeapon = dto.IsWeapon.Value;
            if (dto.ClassId.HasValue) item.ClassId = dto.ClassId.Value;
            if (dto.SlotId.HasValue) item.SlotId = dto.SlotId.Value;
            if (dto.IsActive.HasValue) item.IsActive = dto.IsActive.Value;
            if (dto.CurrentStock.HasValue) item.CurrentStock = dto.CurrentStock.Value;
            if (dto.TradesCount.HasValue) item.TradesCount = dto.TradesCount.Value;
            if (dto.Rating.HasValue) item.Rating = dto.Rating.Value;

            var result = await itemService.UpdateItem(item, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<BaseOperationResult>> DeleteByIdAsync(long id, CancellationToken ct)
    {
        try
        {
            var result = await itemService.DeleteById(id, ct);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
