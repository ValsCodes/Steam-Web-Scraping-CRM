using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("grade")]
public class Grade : BaseModel
{
    [Column("game_id")]
    public long? GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; }
}
