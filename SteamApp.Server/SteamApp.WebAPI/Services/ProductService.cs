

using AutoMapper;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.Entities;
using SteamApp.Models.OperationResults;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Services;

public class ProductService(IProductRepository repository, IMapper mapper) : IProductService
{

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product == null)
            throw new ItemNotFoundException($"Product with ID {id} not found.");

        return mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetListAsync(CancellationToken ct = default)
    {
        var products = await repository.GetListAsync(ct);
        return products.Select(p => mapper.Map<ProductDto>(p));
    }

    public async Task<ItemCreateResult> CreateAsync(CreateProductDto productDto, CancellationToken ct = default)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto));

        if (productDto.DateBought == null)
        {
            productDto.DateBought = DateTime.UtcNow;
        }

        var product = mapper.Map<WatchItem>(productDto);

        var id = await repository.CreateAsync(product, ct);
        return new ItemCreateResult
        {
            Id = id,
            Message = $"Product {id} created successfully"
        };
    }

    public async Task<IEnumerable<ItemCreateResult>> CreateRangeAsync(IEnumerable<CreateProductDto> productDtos, CancellationToken ct = default)
    {
        if (productDtos == null || !productDtos.Any())
            throw new ArgumentException("Product collection cannot be null or empty.", nameof(productDtos));

        var products = productDtos.Select(mapper.Map<WatchItem>);

        var ids = await repository.CreateRangeAsync(products, ct);
        return ids.Select(id => new ItemCreateResult
        {
            Id = id,
            Message = $"Product {id} created successfully"
        });
    }

    public async Task<BaseOperationResult> UpdateAsync(ProductDto productDto, CancellationToken ct = default)
    {
        var product = mapper.Map<WatchItem>(productDto);
        var success = await repository.UpdateAsync(product, ct);

        return new BaseOperationResult
        {
            Success = success,
            Message = success
                ? $"Product {productDto.Id} updated successfully"
                : $"Product {productDto.Id} not found"
        };
    }

    public async Task<IEnumerable<BaseOperationResult>> UpdateRangeAsync(IEnumerable<UpdateProductDto> productDtos, CancellationToken ct = default)
    {

        await Task.CompletedTask;
        throw new NotImplementedException("UpdateRangeAsync is not implemented yet.");

        //if (productDtos == null || !productDtos.Any())
        //    throw new ArgumentException("Product collection cannot be null or empty.", nameof(productDtos));

        //var results = await _repository.UpdateRangeAsync(productDtos, ct);
        //return productDtos.Select((dto, idx) => new OperationResult
        //{
        //    Success = results.ElementAt(idx),
        //    Message = results.ElementAt(idx)
        //        ? $"Product {dto.Id} updated successfully"
        //        : $"Product {dto.Id} not found"
        //});
    }

    public async Task<BaseOperationResult> DeleteAsync(long id, CancellationToken ct = default)
    {
        var success = await repository.DeleteAsync(id, ct);

        return new BaseOperationResult
        {
            Success = success,
            Message = success
                ? $"Product {id} deleted successfully"
                : $"Product {id} not found"
        };
    }

    public async Task<IEnumerable<BaseOperationResult>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        if (ids == null || !ids.Any())
            throw new ArgumentException("ID collection cannot be null or empty.", nameof(ids));

        var results = await repository.DeleteRangeAsync(ids, ct);

        return ids.Select((id, idx) => new BaseOperationResult
        {
            Success = results.ElementAt(idx),
            Message = results.ElementAt(idx)
                ? $"Product {id} deleted successfully"
                : $"Product {id} not found"
        });
    }
}
