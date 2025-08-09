using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("item")]
public class Item : BaseModel
{
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("game_id")]
    public long GameId { get; set; }
    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; }

    [Column("game_url_id")]
    public long GameUrlId { get; set; }
    [ForeignKey(nameof(GameUrlId))]
    public virtual GameUrl GameUrl { get; set; }

    // To be removed
    [Column("is_weapon")]
    public bool IsWeapon { get; set; }

    // To be removed
    [Column("class_id")]
    public long? ClassId { get; set; }
    [ForeignKey(nameof(ClassId))]
    public virtual Class Class { get; set; } 
    
    // To be removed
    [Column("slot_id")]
    public long? SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public virtual Slot? Slot { get; set; }

    [Column("current_stock")]
    public int? CurrentStock { get; set; }

    [Column("trades_count")]
    public int? TradesCount { get; set; }

    [Column("rating")]
    public short? Rating { get; set; }

    [Column("is_favorite")]
    public bool IsFavorite { get; set; }

    [JsonIgnore]
    public virtual ICollection<ItemSkins> ItemSkins { get; set; } = [];
}
