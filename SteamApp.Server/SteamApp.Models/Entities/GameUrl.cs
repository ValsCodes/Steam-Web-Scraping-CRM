using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game_url")]
    public sealed class GameUrl
    {
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

        public ICollection<Product> Products { get; set; } = [];

        public ICollection<Pixel> Pixels { get; set; } = [];

        public ICollection<WatchList> WatchLists { get; set; } = [];
    }
}
