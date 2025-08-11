using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("skin")]
public class Skin : BaseModel
{
    [Column("is_war_paint")]
    public bool IsWarPaint { get; set; }

    [Column("quality_id")]
    public virtual short? QualityId { get; set; }
    [ForeignKey(nameof(QualityId))]
    public virtual Quality? Quality { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [JsonIgnore]
    public virtual ICollection<ItemSkins> ItemSkins { get; set; } = [];
}
