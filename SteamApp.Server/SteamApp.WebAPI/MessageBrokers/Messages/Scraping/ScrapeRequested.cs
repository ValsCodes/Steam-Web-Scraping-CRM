namespace SteamApp.WebAPI.MessageBrokers.Messages.Scraping;

public sealed record ScrapeRequested(
    long HistoryId,
    string UserId,
    long GameUrlId,
    short Page,
    string Endpoint,
    string ScrapeType,
    DateTime RequestedAtUtc,
    string CorrelationId);
