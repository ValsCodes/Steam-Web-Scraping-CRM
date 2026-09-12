using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.GameUrlProduct;

public sealed class GameUrlProductStockHistoryDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long GameUrlId { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public GameUrlProductStockOperationEnum Operation { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
