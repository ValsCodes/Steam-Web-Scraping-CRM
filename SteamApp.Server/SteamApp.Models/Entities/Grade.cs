using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("grade")]
public class Grade : BaseModel
{

    [Column("game_id")]
    public virtual long? GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; }

    [JsonIgnore]
    public virtual ICollection<ItemSkins> ItemSkins { get; set; } = [];
}
