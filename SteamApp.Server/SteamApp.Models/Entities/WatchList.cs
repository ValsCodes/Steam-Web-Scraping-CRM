using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("watch_list")]
    public sealed class WatchList
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("product_id")]
        [ForeignKey(nameof(Product))]
        public long ProductId { get; set; }
        [InverseProperty(nameof(Product.WatchLists))]
        public Product Product { get; set; }

        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.WatchLists))]
        public GameUrl GameUrl { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("batch_number")]
        public long? BatchNumber { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("rd")]
        public DateOnly ReleaseDate { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }

}
