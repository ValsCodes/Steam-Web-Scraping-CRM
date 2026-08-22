using SteamApp.Application.DTOs.Product;
namespace SteamApp.Application.OperationResults
{
    public class ItemCreateResult : BaseOperationResult
    {
        public ProductDto? Created { get; set; }
    }
}
