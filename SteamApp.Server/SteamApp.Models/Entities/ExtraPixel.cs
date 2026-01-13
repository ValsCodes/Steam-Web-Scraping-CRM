using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("extra_pixel")]
    public sealed class ExtraPixel
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_url_id")]
        public long GameUrlId { get; set; }

        [Column("pixel_value")]
        public long PixelValue { get; set; }
    }
}
