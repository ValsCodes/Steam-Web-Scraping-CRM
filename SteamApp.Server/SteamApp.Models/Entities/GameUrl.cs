using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game")]
    public class GameUrl : BaseModel
    {
        public virtual string Url { get; set; }

        [Column("game_id")]
        public virtual long GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public virtual Game Game { get; set; }

        public virtual ICollection<Item> Items { get; set; } = [];
    }
}
