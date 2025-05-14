using SteamApp.Infrastructure.DTOs.Product;

namespace SteamApp.Infrastructure
{
    public class CreateProductResult : OperationResult
    {
        public long Id { get; set; }
        public ProductDto? Created { get; set; }
    }
}
