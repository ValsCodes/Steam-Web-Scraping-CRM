using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SteamApp.Domain.Entities
{
    [PrimaryKey(nameof(GameUrlId), nameof(ProductId))]
    [Table("game_url_products")]
    public class GameUrlProducts
    {
        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.GameUrlsProducts))]
        public GameUrl GameUrl { get; set; }

        [Column("product_id")]
        [ForeignKey(nameof(Product))]
        public long ProductId { get; set; }
        [InverseProperty(nameof(Product.GameUrlsProducts))]
        public Product Product { get; set; }
    }
}
