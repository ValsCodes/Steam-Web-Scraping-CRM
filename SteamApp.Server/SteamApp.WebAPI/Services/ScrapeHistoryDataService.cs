using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;

namespace SteamApp.WebAPI.Services;

public sealed class ScrapeHistoryDataService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IScrapeHistoryDataService
{
    public async Task<IReadOnlyList<ScrapeHistorySummaryDto>> GetHistoryAsync(
        string userId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var historyRows = await db.AutomatedScrapeHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.Endpoint,
                x.ScrapeType,
                x.GameUrlId,
                x.Page,
                x.ResultCount,
                x.Date,
                x.IsHaveError,
                x.Status,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.CorrelationId
            })
            .ToListAsync(cancellationToken);

        var gameUrlIds = historyRows
            .Select(x => x.GameUrlId)
            .Distinct()
            .ToList();

        var gameUrlNames = await db.GameUrls
            .AsNoTracking()
            .Where(x => x.UserId == userId && gameUrlIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return historyRows
            .Select(x => new ScrapeHistorySummaryDto
            {
                Id = x.Id,
                Endpoint = x.Endpoint,
                ScrapeType = x.ScrapeType,
                GameUrlId = x.GameUrlId,
                GameUrlName = gameUrlNames.GetValueOrDefault(x.GameUrlId),
                Page = x.Page,
                ResultCount = x.ResultCount,
                Date = x.Date,
                IsHaveError = x.IsHaveError,
                Status = x.Status,
                StartedAtUtc = x.StartedAtUtc,
                CompletedAtUtc = x.CompletedAtUtc,
                CorrelationId = x.CorrelationId
            })
            .ToList();
    }

    public async Task<ScrapeHistoryDetailDto?> GetDetailAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var detail = await db.AutomatedScrapeHistories
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ScrapeHistoryDetailDto
            {
                Id = x.Id,
                Endpoint = x.Endpoint,
                ScrapeType = x.ScrapeType,
                GameUrlId = x.GameUrlId,
                Page = x.Page,
                ResultCount = x.ResultCount,
                Date = x.Date,
                IsHaveError = x.IsHaveError,
                Status = x.Status,
                StartedAtUtc = x.StartedAtUtc,
                CompletedAtUtc = x.CompletedAtUtc,
                CorrelationId = x.CorrelationId,
                SetupJson = x.SetupJson,
                ResultsJson = x.ResultsJson,
                ErrorText = x.ErrorText
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null)
        {
            return null;
        }

        detail.GameUrlName = await db.GameUrls
            .AsNoTracking()
            .Where(x => x.Id == detail.GameUrlId && x.UserId == userId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return detail;
    }

    public async Task<ScrapeHistoryRerunSource?> GetRerunSourceAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await db.AutomatedScrapeHistories
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ScrapeHistoryRerunSource(
                x.Id,
                x.Endpoint,
                x.ScrapeType,
                x.GameUrlId,
                x.Page))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OwnedGameUrlSnapshot?> GetOwnedGameUrlSnapshotAsync(
        long gameUrlId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await db.GameUrls
            .AsNoTracking()
            .Where(x => x.Id == gameUrlId && x.UserId == userId)
            .Select(x => new OwnedGameUrlSnapshot(
                x.Id,
                x.Name,
                x.GameId,
                x.Game.Name,
                x.ScrapingModeId,
                x.ScrapingMode != null ? x.ScrapingMode.Name : null,
                x.PartialUrl,
                x.StartPage,
                x.EndPage,
                x.PixelX,
                x.PixelY,
                x.PixelImageWidth,
                x.PixelImageHeight))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AutomatedScrapeHistory> AddHistoryAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        object? results,
        string? errorText,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var now = DateTime.UtcNow;
        var hasError = !string.IsNullOrWhiteSpace(errorText);
        var history = new AutomatedScrapeHistory
        {
            UserId = userId,
            Endpoint = endpoint,
            ScrapeType = scrapeType,
            GameUrlId = gameUrl.GameUrlId,
            Page = page,
            SetupJson = JsonConvert.SerializeObject(CreateSetup(gameUrl, page, endpoint, scrapeType, now)),
            ResultsJson = hasError ? null : JsonConvert.SerializeObject(results ?? Array.Empty<WatchItemDto>()),
            ResultCount = hasError ? 0 : GetResultCount(results),
            Date = now,
            ErrorText = errorText,
            IsHaveError = hasError,
            Status = hasError ? ScrapeJobStatusEnum.Failed : ScrapeJobStatusEnum.Succeeded,
            StartedAtUtc = now,
            CompletedAtUtc = now
        };

        db.AutomatedScrapeHistories.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        return history;
    }

    public async Task<AutomatedScrapeHistory> CreateQueuedHistoryAsync(
        string userId,
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        string correlationId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var now = DateTime.UtcNow;
        var history = new AutomatedScrapeHistory
        {
            UserId = userId,
            Endpoint = endpoint,
            ScrapeType = scrapeType,
            GameUrlId = gameUrl.GameUrlId,
            Page = page,
            SetupJson = JsonConvert.SerializeObject(CreateSetup(gameUrl, page, endpoint, scrapeType, now)),
            ResultsJson = null,
            ResultCount = 0,
            Date = now,
            ErrorText = null,
            IsHaveError = false,
            Status = ScrapeJobStatusEnum.Queued,
            CorrelationId = correlationId
        };

        db.AutomatedScrapeHistories.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        return history;
    }

    public async Task<ScrapeHistoryJob?> GetJobAsync(
        long historyId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await db.AutomatedScrapeHistories
            .AsNoTracking()
            .Where(x => x.Id == historyId)
            .Select(x => new ScrapeHistoryJob(
                x.Id,
                x.UserId,
                x.Endpoint,
                x.ScrapeType,
                x.GameUrlId,
                x.Page,
                x.Status,
                x.CorrelationId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ScrapeJobStatusEnum?> MarkRunningAsync(
        long historyId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var history = await db.AutomatedScrapeHistories
            .FirstOrDefaultAsync(x => x.Id == historyId, cancellationToken);

        if (history is null)
        {
            return null;
        }

        if (history.Status is ScrapeJobStatusEnum.Succeeded or ScrapeJobStatusEnum.Failed)
        {
            return history.Status;
        }

        if (history.Status == ScrapeJobStatusEnum.Queued)
        {
            history.Status = ScrapeJobStatusEnum.Running;
            history.StartedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return history.Status;
    }

    public async Task<AutomatedScrapeHistory?> MarkSucceededAsync(
        long historyId,
        object? results,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var history = await db.AutomatedScrapeHistories
            .FirstOrDefaultAsync(x => x.Id == historyId, cancellationToken);

        if (history is null)
        {
            return null;
        }

        if (history.Status is ScrapeJobStatusEnum.Succeeded or ScrapeJobStatusEnum.Failed)
        {
            return history;
        }

        var now = DateTime.UtcNow;
        history.ResultsJson = JsonConvert.SerializeObject(results ?? Array.Empty<WatchItemDto>());
        history.ResultCount = GetResultCount(results);
        history.ErrorText = null;
        history.IsHaveError = false;
        history.Status = ScrapeJobStatusEnum.Succeeded;
        history.StartedAtUtc ??= now;
        history.CompletedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task<AutomatedScrapeHistory?> MarkFailedAsync(
        long historyId,
        string errorText,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var history = await db.AutomatedScrapeHistories
            .FirstOrDefaultAsync(x => x.Id == historyId, cancellationToken);

        if (history is null)
        {
            return null;
        }

        if (history.Status is ScrapeJobStatusEnum.Succeeded or ScrapeJobStatusEnum.Failed)
        {
            return history;
        }

        var now = DateTime.UtcNow;
        history.ResultsJson = null;
        history.ResultCount = 0;
        history.ErrorText = errorText;
        history.IsHaveError = true;
        history.Status = ScrapeJobStatusEnum.Failed;
        history.StartedAtUtc ??= now;
        history.CompletedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
        return history;
    }

    private static int GetResultCount(object? results)
    {
        return results switch
        {
            null => 0,
            string => 0,
            IEnumerable<WatchItemDto> watchItems => watchItems.Count(),
            System.Collections.IEnumerable enumerable => enumerable.Cast<object>().Count(),
            _ => 1
        };
    }

    private static ScrapeHistorySetupDto CreateSetup(
        OwnedGameUrlSnapshot gameUrl,
        short page,
        string endpoint,
        string scrapeType,
        DateTime requestedAtUtc)
    {
        return new ScrapeHistorySetupDto
        {
            Endpoint = endpoint,
            ScrapeType = scrapeType,
            GameUrlId = gameUrl.GameUrlId,
            GameUrlName = gameUrl.GameUrlName,
            GameId = gameUrl.GameId,
            GameName = gameUrl.GameName,
            ScrapingModeId = gameUrl.ScrapingModeId,
            ScrapingModeName = gameUrl.ScrapingModeName,
            PartialUrl = gameUrl.PartialUrl,
            StartPage = gameUrl.StartPage,
            EndPage = gameUrl.EndPage,
            PixelX = gameUrl.PixelX,
            PixelY = gameUrl.PixelY,
            PixelImageWidth = gameUrl.PixelImageWidth,
            PixelImageHeight = gameUrl.PixelImageHeight,
            Page = page,
            RequestedAt = requestedAtUtc
        };
    }
}
