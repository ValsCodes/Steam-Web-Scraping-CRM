using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("game_url")]
    public sealed class GameUrl
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("game_id")]
        [ForeignKey(nameof(Game))]
        public long GameId { get; set; }
        [InverseProperty(nameof(Game.GameUrls))]
        public Game Game { get; set; } = null!;

        [Column("scraping_mode_id")]
        [ForeignKey(nameof(ScrapingMode))]
        public long? ScrapingModeId { get; set; }
        [InverseProperty(nameof(ScrapingMode.GameUrls))]
        public ScrapingMode? ScrapingMode { get; set; }

        [Column("partial_url")]
        public string? PartialUrl { get; set; }

        [Column("start_page")]
        public int? StartPage { get; set; }

        [Column("end_page")]
        public int? EndPage { get; set; }

        [Column("pixel_x")]
        public int? PixelX { get; set; }

        [Column("pixel_y")]
        public int? PixelY { get; set; }

        [Column("pixel_image_width")]
        public int? PixelImageWidth { get; set; }

        [Column("pixel_image_height")]
        public int? PixelImageHeight{ get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [MaxLength(450)]
        [Column("user_id")]
        public string? UserId { get; set; }

        public ICollection<GameUrlPixels> GameUrlsPixels { get; set; } = [];

        public ICollection<GameUrlProducts> GameUrlsProducts { get; set; } = [];

        public ICollection<WatchList> WatchLists { get; set; } = [];
    }
}
