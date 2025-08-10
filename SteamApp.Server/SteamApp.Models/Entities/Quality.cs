using SteamApp.Models.Entities.ManyToMany;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("quality")]
public class Quality : BaseModel<short>
{
    [Column("game_id")]
    public long GameId { get; set; }
    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; }

    [Column("is_skin_quality")]
    public bool IsSkinQuality { get; set; }

    public ICollection<WatchItem> Products { get; set; } = [];
    public ICollection<Skin> Skins { get; set; } = [];
    public ICollection<ItemQualities> ItemQualities { get; set; } = [];
}
