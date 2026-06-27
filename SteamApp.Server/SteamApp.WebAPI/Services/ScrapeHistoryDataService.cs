using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.ScrapeHistory;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Domain.Entities;
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
                x.IsHaveError
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
                IsHaveError = x.IsHaveError
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
        var setup = new ScrapeHistorySetupDto
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
            RequestedAt = now
        };

        var hasError = !string.IsNullOrWhiteSpace(errorText);
        var history = new AutomatedScrapeHistory
        {
            UserId = userId,
            Endpoint = endpoint,
            ScrapeType = scrapeType,
            GameUrlId = gameUrl.GameUrlId,
            Page = page,
            SetupJson = JsonConvert.SerializeObject(setup),
            ResultsJson = hasError ? null : JsonConvert.SerializeObject(results ?? Array.Empty<WatchItemDto>()),
            ResultCount = hasError ? 0 : GetResultCount(results),
            Date = now,
            ErrorText = errorText,
            IsHaveError = hasError
        };

        db.AutomatedScrapeHistories.Add(history);
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
}
