using SteamApp.Contracts.ScrapeHistory;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.Abstractions.Persistence;

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

    Task<IReadOnlyList<long>> GetRecoverableJobIdsAsync(
        DateTime queuedBeforeUtc, DateTime nowUtc, CancellationToken cancellationToken);

    Task<bool> TryClaimJobAsync(
        long historyId, Guid claimId, DateTime nowUtc, DateTime expiresAtUtc, CancellationToken cancellationToken);

    Task<bool> RenewClaimAsync(
        long historyId, Guid claimId, DateTime nowUtc, DateTime expiresAtUtc, CancellationToken cancellationToken);

    Task<bool> CompleteJobAsync(
        long historyId, Guid claimId, object? results, string? errorText, CancellationToken cancellationToken);
}
