using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("pixel")]
    public sealed class Pixel
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.Pixels))] 
        public GameUrl GameUrl { get; set; }

        [Column("value")]
        public long Value { get; set; }
    }
}
