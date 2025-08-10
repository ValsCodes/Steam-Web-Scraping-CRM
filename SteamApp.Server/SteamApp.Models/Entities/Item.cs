using SteamApp.Models.Entities.ManyToMany;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("item")]
public class Item : BaseModel
{
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("game_id")]
    public long GameId { get; set; }
    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; }

    [Column("game_url_id")]
    public long GameUrlId { get; set; }
    [ForeignKey(nameof(GameUrlId))]
    public GameUrl GameUrl { get; set; }

    [Column("current_stock")]
    public int? CurrentStock { get; set; }

    [Column("trades_count")]
    public int? TradesCount { get; set; }

    [Column("rating")]
    public short? Rating { get; set; }

    [Column("is_favorite")]
    public bool IsFavorite { get; set; }

    public ICollection<WatchItem> WatchItems { get; set; } = [];
    public ICollection<ItemGameAddOns> ItemGameAddOns { get; set; } = [];
    public ICollection<ItemQualities> ItemQualities { get; set; } = [];

    public ICollection<ItemClasses> ItemClasses { get; set; } = [];
    public ICollection<ItemSlots> ItemSlots { get; set; } = [];
    public ICollection<Skin> Skins { get; set; } = [];
}
