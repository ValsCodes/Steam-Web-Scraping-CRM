using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game_urls")]
    public sealed class GameUrl
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_id")]
        public long GameId { get; set; }

        [Column("partial_url")]
        public string PartialUrl { get; set; }

        [Column("is_batch_url")]
        public bool IsBatchUrl { get; set; }

        [Column("start_page")]
        public int? StartPage { get; set; }

        [Column("end_page")]
        public int? EndPage { get; set; }

        [Column("is_pixel_scrape")]
        public bool IsPixelScrape { get; set; }

        public ICollection<Product> Products { get; set; } = [];
        public ICollection<ExtraPixel> ExtraPixels { get; set; } = [];
    }
}
