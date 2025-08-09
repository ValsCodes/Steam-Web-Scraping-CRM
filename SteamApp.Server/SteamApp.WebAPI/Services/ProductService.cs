using AutoMapper;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product == null)
            throw new ItemNotFoundException($"Product with ID {id} not found.");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetListAsync(CancellationToken ct = default)
    {
        var products = await _repository.GetListAsync(ct);
        return products.Select(p => _mapper.Map<ProductDto>(p));
    }

    public async Task<CreateProductResult> CreateAsync(CreateProductDto productDto, CancellationToken ct = default)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto));

        if (productDto.DateBought == null)
        {
            productDto.DateBought = DateTime.UtcNow;
        }

        var product = _mapper.Map<WatchItem>(productDto);

        var id = await _repository.CreateAsync(product, ct);
        return new CreateProductResult
        {
            Id = id,
            Message = $"Product {id} created successfully"
        };
    }

    public async Task<IEnumerable<CreateProductResult>> CreateRangeAsync(IEnumerable<CreateProductDto> productDtos, CancellationToken ct = default)
    {
        if (productDtos == null || !productDtos.Any())
            throw new ArgumentException("Product collection cannot be null or empty.", nameof(productDtos));

        var products = productDtos.Select(_mapper.Map<WatchItem>);

        var ids = await _repository.CreateRangeAsync(products, ct);
        return ids.Select(id => new CreateProductResult
        {
            Id = id,
            Message = $"Product {id} created successfully"
        });
    }

    public async Task<OperationResult> UpdateAsync(ProductDto productDto, CancellationToken ct = default)
    {
        var product = _mapper.Map<WatchItem>(productDto);
        var success = await _repository.UpdateAsync(product, ct);

        return new OperationResult
        {
            Success = success,
            Message = success
                ? $"Product {productDto.Id} updated successfully"
                : $"Product {productDto.Id} not found"
        };
    }

    public async Task<IEnumerable<OperationResult>> UpdateRangeAsync(IEnumerable<UpdateProductDto> productDtos, CancellationToken ct = default)
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

    public async Task<OperationResult> DeleteAsync(long id, CancellationToken ct = default)
    {
        var success = await _repository.DeleteAsync(id, ct);

        return new OperationResult
        {
            Success = success,
            Message = success
                ? $"Product {id} deleted successfully"
                : $"Product {id} not found"
        };
    }

    public async Task<IEnumerable<OperationResult>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        if (ids == null || !ids.Any())
            throw new ArgumentException("ID collection cannot be null or empty.", nameof(ids));

        var results = await _repository.DeleteRangeAsync(ids, ct);

        return ids.Select((id, idx) => new OperationResult
        {
            Success = results.ElementAt(idx),
            Message = results.ElementAt(idx)
                ? $"Product {id} deleted successfully"
                : $"Product {id} not found"
        });
    }
}
