namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed record ScrapeHistoryRerunSource(
    long Id,
    string Endpoint,
    string ScrapeType,
    long GameUrlId,
    short Page);
