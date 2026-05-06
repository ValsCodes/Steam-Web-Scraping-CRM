namespace SteamApp.Application.DTOs.GameUrl
{
    public sealed class GameUrlUpdateDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long? ScrapingModeId { get; set; }
        public string? PartialUrl { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public int? PixelX { get; set; }
        public int? PixelY { get; set; }
        public int? PixelImageWidth { get; set; }
        public int? PixelImageHeight { get; set; }
    }
}
