using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("game")]
    public class Game
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("base_url")]
        public string? BaseUrl { get; set; }

        [Column("page_url")]
        public string? PageUrl { get; set; }

        public ICollection<GameUrl> GameUrls { get; set; } = [];
        public ICollection<WishList> WishLists{ get; set; } = [];
        public ICollection<Product> Products { get; set; } = [];
        public ICollection<Pixel> Pixels { get; set; } = [];
        public ICollection<GameAddOn> GameAddOns { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];
    }
}
