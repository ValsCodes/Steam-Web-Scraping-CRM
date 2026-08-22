namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed record OwnedGameUrlSnapshot(
    long GameUrlId,
    string? GameUrlName,
    long GameId,
    string? GameName,
    long? ScrapingModeId,
    string? ScrapingModeName,
    string? PartialUrl,
    int? StartPage,
    int? EndPage,
    int? PixelX,
    int? PixelY,
    int? PixelImageWidth,
    int? PixelImageHeight);
