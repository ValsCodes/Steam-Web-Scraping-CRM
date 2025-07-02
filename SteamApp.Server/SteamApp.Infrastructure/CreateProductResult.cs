using SteamApp.Models.DTOs.Product;

namespace SteamApp.Infrastructure
{
    public class CreateProductResult : OperationResult
    {
        public ProductDto? Created { get; set; }
    }
}
