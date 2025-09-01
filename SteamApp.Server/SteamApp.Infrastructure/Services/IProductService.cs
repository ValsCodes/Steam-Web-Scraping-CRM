using SteamApp.Models.DTOs.Product;
using SteamApp.Models.OperationResults;

namespace SteamApp.Infrastructure.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<ProductDto>> GetListAsync(CancellationToken ct = default);

        Task<ItemCreateResult> CreateAsync(CreateProductDto product, CancellationToken ct = default);
        Task<IEnumerable<ItemCreateResult>> CreateRangeAsync(IEnumerable<CreateProductDto> products, CancellationToken ct = default);

        Task<BaseOperationResult> UpdateAsync(ProductDto productDto, CancellationToken ct = default);
        Task<IEnumerable<BaseOperationResult>> UpdateRangeAsync(IEnumerable<UpdateProductDto> products, CancellationToken ct = default);

        Task<BaseOperationResult> DeleteAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<BaseOperationResult>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    }
}
