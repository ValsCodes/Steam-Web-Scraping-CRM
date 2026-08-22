namespace SteamApp.WebAPI.Scraping;

public sealed record ScrapeErrorResult(
    int StatusCode,
    string Message,
    LogLevel LogLevel);
