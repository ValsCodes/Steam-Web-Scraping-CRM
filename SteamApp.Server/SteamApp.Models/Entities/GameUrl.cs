using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game_url")]
    public class GameUrl : BaseModel
    {
        public string Url { get; set; }

        [Column("game_id")]
        public long GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        public ICollection<Item> Items { get; set; } = [];
    }
}
