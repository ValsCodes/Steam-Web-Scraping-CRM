using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;

namespace SteamApp.Interfaces.Services;

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

    Task<AutomatedScrapeHistory> CreateQueuedHistoryAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        string correlationId,
        CancellationToken cancellationToken);

    Task<ScrapeHistoryJob?> GetJobAsync(
        long historyId,
        CancellationToken cancellationToken);

    Task<ScrapeJobStatusEnum?> MarkRunningAsync(
        long historyId,
        CancellationToken cancellationToken);

    Task<AutomatedScrapeHistory?> MarkSucceededAsync(
        long historyId,
        object? results,
        CancellationToken cancellationToken);

    Task<AutomatedScrapeHistory?> MarkFailedAsync(
        long historyId,
        string errorText,
        CancellationToken cancellationToken);
}
