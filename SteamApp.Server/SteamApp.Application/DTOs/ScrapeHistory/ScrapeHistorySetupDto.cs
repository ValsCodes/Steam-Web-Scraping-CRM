namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed class ScrapeHistorySetupDto
{
    public string Endpoint { get; set; } = string.Empty;
    public string ScrapeType { get; set; } = string.Empty;
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long? ScrapingModeId { get; set; }
    public string? ScrapingModeName { get; set; }
    public string? PartialUrl { get; set; }
    public int? StartPage { get; set; }
    public int? EndPage { get; set; }
    public int? PixelX { get; set; }
    public int? PixelY { get; set; }
    public int? PixelImageWidth { get; set; }
    public int? PixelImageHeight { get; set; }
    public short Page { get; set; }
    public DateTime RequestedAt { get; set; }
}
