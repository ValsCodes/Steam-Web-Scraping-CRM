using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game")]
    public class Game
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("base_url")]
        public string? BaseUrl { get; set; }

        public ICollection<GameUrl> GameUrls { get; set; } = [];
        public ICollection<WishList> WishLists{ get; set; } = [];
        public ICollection<WatchList> WatchLists { get; set; } = [];
    }
}
