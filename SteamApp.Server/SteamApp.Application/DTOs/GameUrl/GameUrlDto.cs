namespace SteamApp.Application.DTOs.GameUrl
{
    public sealed class GameUrlDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long GameId { get; set; }
        public long? ScrapingModeId { get; set; }
        public string? ScrapingModeName { get; set; }
        public string PartialUrl { get; set; } = string.Empty;
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public int? PixelX { get; set; }
        public int? PixelY { get; set; }
        public int? PixelImageWidth { get; set; }
        public int? PixelImageHeight { get; set; }
        public bool IsActive { get; set; }
    }
}
