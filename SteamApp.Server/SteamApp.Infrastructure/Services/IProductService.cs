using SteamApp.Models.DTOs.Product;

namespace SteamApp.Infrastructure.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<ProductDto>> GetListAsync(CancellationToken ct = default);

        Task<CreateProductResult> CreateAsync(CreateProductDto product, CancellationToken ct = default);
        Task<IEnumerable<CreateProductResult>> CreateRangeAsync(IEnumerable<CreateProductDto> products, CancellationToken ct = default);

        Task<OperationResult> UpdateAsync(ProductDto productDto, CancellationToken ct = default);
        Task<IEnumerable<OperationResult>> UpdateRangeAsync(IEnumerable<UpdateProductDto> products, CancellationToken ct = default);

        Task<OperationResult> DeleteAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<OperationResult>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    }
}
