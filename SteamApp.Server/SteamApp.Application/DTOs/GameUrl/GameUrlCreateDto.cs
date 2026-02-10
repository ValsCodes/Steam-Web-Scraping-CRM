namespace SteamApp.Application.DTOs.GameUrl
{
    public sealed class GameUrlCreateDto
    {
        public string? Name { get; set; }
        public long GameId { get; set; }
        public string PartialUrl { get; set; }
        public bool IsBatchUrl { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public bool IsPixelScrape { get; set; }
        public int? PixelX { get; set; }
        public int? PixelY { get; set; }
        public int? PixelImageWidth { get; set; }
        public int? PixelImageHeight { get; set; }
        public bool IsPublicApi { get; set; }
    }
}
