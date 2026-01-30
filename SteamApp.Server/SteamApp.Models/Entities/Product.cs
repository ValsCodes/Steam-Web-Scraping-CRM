using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("product")]
    public sealed class Product
    {
        [Key]
        [Column("id")]
        public long Id { get;  set; }

        [Column("game_id")]
        [ForeignKey(nameof(Game))]
        public long GameId { get; set; }
        [InverseProperty(nameof(Game.Products))] 
        public Game Game { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        public ICollection<GameUrlProducts> GameUrlsProducts { get; set; } = [];

        public ICollection<WatchList> WatchLists { get; set; } = [];
    }
}
