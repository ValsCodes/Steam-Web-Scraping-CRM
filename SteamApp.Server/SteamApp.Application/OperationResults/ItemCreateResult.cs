using SteamApp.Application.DTOs.Product;
using SteamApp.Models.OperationResults;

namespace SteamApp.Infrastructure
{
    public class ItemCreateResult : BaseOperationResult
    {
        public ProductDto? Created { get; set; }
    }
}
