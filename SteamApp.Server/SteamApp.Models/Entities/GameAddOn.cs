using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    // TODO decalare multiple keys in DataSet

    [Table("game_add_on")]
    public class GameAddOn : BaseModel
    {
        [Column("game_id")]
        public virtual long GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public virtual Game Game { get; set; }
    }
}
