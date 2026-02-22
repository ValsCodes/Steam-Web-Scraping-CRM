using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [PrimaryKey(nameof(GameUrlId), nameof(PixelId))]
    [Table("game_url_pixels")]
    public class GameUrlPixels
    {
        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.GameUrlsPixels))]
        public GameUrl GameUrl { get; set; }

        [Column("pixel_id")]
        [ForeignKey(nameof(Pixel))]
        public long PixelId { get; set; }
        [InverseProperty(nameof(Pixel.GameUrlsPixels))]
        public Pixel Pixel { get; set; }
    }
}
