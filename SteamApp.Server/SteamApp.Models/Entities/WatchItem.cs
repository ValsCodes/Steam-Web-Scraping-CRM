using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("watch_item")]
public class WatchItem : BaseModel
{
    [Column("item_id")]
    public long? ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; }

    [Column("last_check_date")]
    public DateTime LastCheckDate { get; set; }

    [Column("price")]
    public decimal? Price { get; set; }

    public ICollection<Target> Targets { get; set; } = [];
}
