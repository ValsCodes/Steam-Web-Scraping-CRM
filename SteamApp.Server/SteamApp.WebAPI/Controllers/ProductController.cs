﻿using Microsoft.AspNetCore.Mvc;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs.Product;

namespace SteamApp.WebAPI.Controllers;

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
    public async Task<ActionResult<OperationResult>> Update([FromBody] UpdateProductDto dto)
    {
        if (dto == null)
            return BadRequest("No data provided.");

        var product = await _productService.GetByIdAsync(dto.Id);
        if (product == null)
            return NotFound();

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.QualityId.HasValue) product.QualityId = (short)dto.QualityId;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.DateBought.HasValue) product.DateBought = dto.DateBought ?? new DateTime();
        if (dto.DateSold.HasValue) product.DateSold = dto.DateSold;
        if (dto.CostPrice.HasValue) product.CostPrice = dto.CostPrice.Value;
        if (dto.TargetSellPrice1.HasValue) product.TargetSellPrice1 = dto.TargetSellPrice1.Value;
        if (dto.TargetSellPrice2.HasValue) product.TargetSellPrice2 = dto.TargetSellPrice2.Value;
        if (dto.TargetSellPrice3.HasValue) product.TargetSellPrice3 = dto.TargetSellPrice3.Value;
        if (dto.TargetSellPrice4.HasValue) product.TargetSellPrice4 = dto.TargetSellPrice4.Value;
        if (dto.SoldPrice.HasValue) product.SoldPrice = dto.SoldPrice.Value;
        if (dto.IsHat.HasValue) product.IsHat = dto.IsHat.Value;
        if (dto.IsWeapon.HasValue) product.IsWeapon = dto.IsWeapon.Value;
        if (dto.IsSold.HasValue) product.IsSold = dto.IsSold.Value;

        await _productService.UpdateAsync(product);
        return NoContent();
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
