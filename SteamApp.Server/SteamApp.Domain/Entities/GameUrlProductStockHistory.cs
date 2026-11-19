using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class GameUrlProductStockHistory
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long GameUrlId { get; set; }

    public int PreviousStock { get; set; }

    public int NewStock { get; set; }

    public GameUrlProductStockOperationEnum Operation { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
}
