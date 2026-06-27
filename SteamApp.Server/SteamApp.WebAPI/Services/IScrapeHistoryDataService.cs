using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Services;

public interface IScrapeHistoryDataService
{
    Task<IReadOnlyList<ScrapeHistorySummaryDto>> GetHistoryAsync(
        string userId,
        int take,
        CancellationToken cancellationToken);

    Task<ScrapeHistoryDetailDto?> GetDetailAsync(
        long id,
        string userId,
        CancellationToken cancellationToken);

    Task<ScrapeHistoryRerunSource?> GetRerunSourceAsync(
        long id,
        string userId,
        CancellationToken cancellationToken);

    Task<OwnedGameUrlSnapshot?> GetOwnedGameUrlSnapshotAsync(
        long gameUrlId,
        string userId,
        CancellationToken cancellationToken);

    Task<AutomatedScrapeHistory> AddHistoryAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        object? results,
        string? errorText,
        CancellationToken cancellationToken);
}

public sealed record ScrapeHistoryRerunSource(
    long Id,
    string Endpoint,
    string ScrapeType,
    long GameUrlId,
    short Page);

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
