using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("game_url_product_stock_history")]
public sealed class GameUrlProductStockHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("game_url_id")]
    public long GameUrlId { get; set; }

    [Column("previous_stock")]
    public int PreviousStock { get; set; }

    [Column("new_stock")]
    public int NewStock { get; set; }

    [Column("operation")]
    public GameUrlProductStockOperationEnum Operation { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;
}
