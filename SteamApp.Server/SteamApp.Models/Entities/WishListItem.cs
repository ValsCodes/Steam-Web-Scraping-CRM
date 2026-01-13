using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("wish_list")]
    public sealed class WishListItem
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_id")]
        public long GameId { get; set; }

        [Column("price")]
        public double? Price { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
