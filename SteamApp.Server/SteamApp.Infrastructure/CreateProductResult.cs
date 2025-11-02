using SteamApp.Models.DTOs.Product;

namespace SteamApp.Models.OperationResults
{
    public class ItemCreateResult : BaseOperationResult
    {
        public ProductDto? Created { get; set; }
    }
}
