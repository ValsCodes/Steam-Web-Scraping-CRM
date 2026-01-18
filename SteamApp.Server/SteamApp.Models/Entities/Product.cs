using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("product")]
    public sealed class Product
    {
        [Column("id")]
        public long Id { get;  set; }

        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.Products))] 
        public GameUrl GameUrl { get; set; }

        [Column("name")]
        public string? Name { get; set; }
    }
}
