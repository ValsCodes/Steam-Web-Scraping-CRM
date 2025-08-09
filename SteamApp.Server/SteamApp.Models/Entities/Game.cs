using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game")]
    public class Game : BaseModel
    {
        public virtual ICollection<GameUrl> GameUrls { get; set; } = [];
    }
}
