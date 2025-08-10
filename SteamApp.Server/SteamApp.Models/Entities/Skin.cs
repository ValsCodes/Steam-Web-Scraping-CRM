using SteamApp.Models.Entities.ManyToMany;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("skin")]
public class Skin : BaseModel
{
    [Column("item_id")]
    public long ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; }

    [Column("quality_id")]
    public  short? QualityId { get; set; }
    [ForeignKey(nameof(QualityId))]
    public  Quality? Quality { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}
