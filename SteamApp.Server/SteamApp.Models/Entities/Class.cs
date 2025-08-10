using SteamApp.Models.Entities.ManyToMany;
using SteamApp.Models.Entities.OneToOne;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("class")]
public class Class : BaseModel
{
    [Column("game_id")]
    public long GameId { get; set; }
    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; }

    public  ICollection<TeamFortressItem> TeamFotressItems { get; set; } = [];

    public ICollection<ItemClasses>  ItemClasses { get; set; } = [];
}
