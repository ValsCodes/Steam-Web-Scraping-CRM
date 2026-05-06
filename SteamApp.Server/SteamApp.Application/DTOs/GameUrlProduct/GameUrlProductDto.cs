namespace SteamApp.Application.DTOs.GameUrlProduct
{
    public class GameUrlProductDto
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public long GameUrlId { get; set; }

        public string GameUrlName { get; set; } = null!;

        public long? ScrapingModeId { get; set; }

        public string? ScrapingModeName { get; set; }

        public string FullUrl { get; set; } = null!;
    }
}
