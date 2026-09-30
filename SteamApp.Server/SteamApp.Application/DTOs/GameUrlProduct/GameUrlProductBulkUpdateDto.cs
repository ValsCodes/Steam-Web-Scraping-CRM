namespace SteamApp.Application.DTOs.GameUrlProduct;

public sealed class GameUrlProductBulkUpdateDto
{
    public long[]? AddProductIds { get; set; }
    public long[]? RemoveProductIds { get; set; }
}
