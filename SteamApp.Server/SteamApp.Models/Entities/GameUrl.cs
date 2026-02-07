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
        public Game Game { get; set; }

        [Column("partial_url")]
        public string? PartialUrl { get; set; }

        [Column("is_batch_url")]
        public bool IsBatchUrl { get; set; }

        [Column("start_page")]
        public int? StartPage { get; set; }

        [Column("end_page")]
        public int? EndPage { get; set; }

        [Column("is_pixel_scrape")]
        public bool IsPixelScrape { get; set; }

        [Column("pixel_x")]
        public int? PixelX { get; set; }

        [Column("pixel_y")]
        public int? PixelY { get; set; }

        [Column("pixel_image_width")]
        public int? PixelImageWidth { get; set; }

        [Column("pixel_image_height")]
        public int? PixelImageHeight{ get; set; }

        [Column("is_public_api")]
        public bool IsPublicApi { get; set; }

        public ICollection<GameUrlPixels> GameUrlsPixels { get; set; } = [];

        public ICollection<WatchList> WatchLists { get; set; } = [];

        public ICollection<GameUrlProducts> GameUrlsProducts { get; set; } = [];
    }
}
