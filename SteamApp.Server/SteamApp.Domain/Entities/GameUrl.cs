using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities
{
    public sealed class GameUrl
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public long GameId { get; set; }
        public Game Game { get; set; } = null!;

        public long? ItemGroupId { get; set; }
        public ItemGroup? ItemGroup { get; set; }

        public long? ScrapingModeId { get; set; }
        public ScrapingMode? ScrapingMode { get; set; }

        public string? PartialUrl { get; set; }

        public int? StartPage { get; set; }

        public int? EndPage { get; set; }

        public int? PixelX { get; set; }

        public int? PixelY { get; set; }

        public int? PixelImageWidth { get; set; }

        public int? PixelImageHeight{ get; set; }

        public bool IsActive { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public ICollection<GameUrlPixels> GameUrlsPixels { get; set; } = [];

        public ICollection<GameUrlProducts> GameUrlsProducts { get; set; } = [];

        public ICollection<ManualCheckPresetGameUrl> ManualCheckPresets { get; set; } = [];

        public ICollection<WatchList> WatchLists { get; set; } = [];
    }
}
