using Microsoft.AspNetCore.JsonPatch;
using SteamApp.Infrastructure.DTOs.Product;

namespace SteamApp.Infrastructure.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<ProductDto>> GetListAsync(CancellationToken ct = default);

        Task<CreateResult> CreateAsync(CreateProductDto product, CancellationToken ct = default);
        Task<IEnumerable<CreateResult>> CreateRangeAsync(IEnumerable<CreateProductDto> products, CancellationToken ct = default);

        Task<OperationResult> UpdateAsync(long id, JsonPatchDocument<ProductForPatchDto> patchDoc, CancellationToken ct = default);
        Task<IEnumerable<OperationResult>> UpdateRangeAsync(IEnumerable<UpdateProductDto> products, CancellationToken ct = default);

        Task<OperationResult> DeleteAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<OperationResult>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    }
}
