using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.Services;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Controllers;
using SteamApp.WebAPI.Services;
using System.Security.Claims;

namespace SteamApp.Tests.Controllers;

[TestFixture]
public sealed class SteamControllerTests
{
    [Test]
    public async Task ScrapePageAsync_ReturnsCachedValueWithoutCallingService()
    {
        using var cache = TestDb.CreateMemoryCache();
        var cached = new[] { new WatchItemDto { Name = "Cached", Price = 1 } };
        cache.Set(string.Format(CacheKeys.ScrapePage, 1, 2), cached);

        var steamService = new Mock<ISteamService>(MockBehavior.Strict);
        var controller = CreateController(steamService: steamService, cache: cache);

        var result = await controller.ScrapePageAsync(1, 2);

        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.SameAs(cached));
        steamService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapePageAsync_CachesSuccessfulServiceResponse()
    {
        using var cache = TestDb.CreateMemoryCache();
        var response = new[] { new WatchItemDto { Name = "Fresh", Price = 2 } };
        var steamService = new Mock<ISteamService>();
        steamService.Setup(x => x.ScrapePage(1, 2)).ReturnsAsync(response);
        var controller = CreateController(steamService: steamService, cache: cache);

        var result = await controller.ScrapePageAsync(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.ScrapePage, 1, 2), out object? cached), Is.True);
            Assert.That(cached, Is.SameAs(response));
        });
    }

    [Test]
    public async Task ScrapePageAsync_RecordsSuccessfulHistory()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        var response = new[] { new WatchItemDto { Name = "Fresh", Price = 2 } };
        var steamService = new Mock<ISteamService>();
        steamService.Setup(x => x.ScrapePage(1, 2)).ReturnsAsync(response);
        var controller = CreateController(steamService: steamService, database: database);

        var result = await controller.ScrapePageAsync(1, 2);

        var history = db.AutomatedScrapeHistories.Single();
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(history.UserId, Is.EqualTo(TestDb.TestUserId));
            Assert.That(history.Endpoint, Is.EqualTo("scrape-page"));
            Assert.That(history.ScrapeType, Is.EqualTo("Web Scrape"));
            Assert.That(history.GameUrlId, Is.EqualTo(1));
            Assert.That(history.Page, Is.EqualTo(2));
            Assert.That(history.ResultCount, Is.EqualTo(1));
            Assert.That(history.IsHaveError, Is.False);
            Assert.That(history.SetupJson, Does.Contain("Batch URL"));
            Assert.That(history.ResultsJson, Does.Contain("Fresh"));
            Assert.That(history.ErrorText, Is.Null);
        });
    }

    [Test]
    public async Task ScrapePageAsync_RecordsCachedSuccessfulHistory()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        using var cache = TestDb.CreateMemoryCache();
        var cached = new[] { new WatchItemDto { Name = "Cached", Price = 1 } };
        cache.Set(string.Format(CacheKeys.ScrapePage, 1, 2), cached);

        var steamService = new Mock<ISteamService>(MockBehavior.Strict);
        var controller = CreateController(steamService: steamService, cache: cache, database: database);

        var result = await controller.ScrapePageAsync(1, 2);

        var history = db.AutomatedScrapeHistories.Single();
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(history.ResultCount, Is.EqualTo(1));
            Assert.That(history.IsHaveError, Is.False);
            Assert.That(history.ResultsJson, Does.Contain("Cached"));
        });
        steamService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapePageAsync_RejectsNegativePageBeforeCallingService()
    {
        var steamService = new Mock<ISteamService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(steamService: steamService, logger: logger);

        var result = await controller.ScrapePageAsync(1, -1);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        VerifyLogged(logger, LogLevel.Error);
        steamService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapeFromPublicApi_CachesSuccessfulServiceResponse()
    {
        using var cache = TestDb.CreateMemoryCache();
        var response = new[] { new WatchItemDto { Name = "Public", Price = 3 } };
        var steamService = new Mock<ISteamService>();
        steamService.Setup(x => x.ScrapeFromPublicApi(1, 2)).ReturnsAsync(response);
        var controller = CreateController(steamService: steamService, cache: cache);

        var result = await controller.ScrapeFromPublicApi(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.ScrapePublic, 1, 2), out object? cached), Is.True);
            Assert.That(cached, Is.SameAs(response));
        });
    }

    [Test]
    public async Task ScrapeForPixelsAsync_ReturnsBadRequestForJsonSerializationException()
    {
        var steamService = new Mock<ISteamService>();
        steamService
            .Setup(x => x.ScrapeWithPixels(1, 2))
            .ThrowsAsync(new JsonSerializationException("bad listing"));
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(steamService: steamService, logger: logger);

        var result = await controller.ScrapeForPixelsAsync(1, 2);

        var objectResult = result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
            Assert.That(objectResult?.Value, Is.EqualTo("Error: Invalid Listing"));
        });
        VerifyLogged(logger, LogLevel.Warning);
    }

    [Test]
    public async Task ScrapeForPixelsAsync_RecordsInvalidListingErrorHistory()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        var steamService = new Mock<ISteamService>();
        steamService
            .Setup(x => x.ScrapeWithPixels(1, 2))
            .ThrowsAsync(new JsonSerializationException("bad listing"));
        var controller = CreateController(steamService: steamService, database: database);

        var result = await controller.ScrapeForPixelsAsync(1, 2);

        var history = db.AutomatedScrapeHistories.Single();
        var objectResult = result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
            Assert.That(history.Endpoint, Is.EqualTo("scrape-pixels"));
            Assert.That(history.ScrapeType, Is.EqualTo("Pixel Scrape"));
            Assert.That(history.IsHaveError, Is.True);
            Assert.That(history.ErrorText, Is.EqualTo("Error: Invalid Listing"));
            Assert.That(history.ResultsJson, Is.Null);
            Assert.That(history.ResultCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ScrapeForPixelsAsync_ReturnsServerErrorAndLogsForUnexpectedException()
    {
        var steamService = new Mock<ISteamService>();
        steamService
            .Setup(x => x.ScrapeWithPixels(1, 2))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(steamService: steamService, logger: logger);

        var result = await controller.ScrapeForPixelsAsync(1, 2);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        VerifyLogged(logger, LogLevel.Error);
    }

    [Test]
    public async Task CheckWishlistItem_ReturnsCachedValueWithoutCallingService()
    {
        using var cache = TestDb.CreateMemoryCache();
        var cached = new WhishListResponse
        {
            GameName = "Cached",
            CurrentPrice = 1,
            IsPriceReached = true
        };
        cache.Set(string.Format(CacheKeys.WishListItem, 1), cached);

        var wishlistService = new Mock<IWishlistService>(MockBehavior.Strict);
        var controller = CreateController(wishlistService: wishlistService, cache: cache);

        var result = await controller.CheckWithlistItem(1);

        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.SameAs(cached));
        wishlistService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CheckWishlistItem_ReturnsBadRequestForJsonSerializationException()
    {
        var wishlistService = new Mock<IWishlistService>();
        wishlistService
            .Setup(x => x.CheckWishlistItem(1))
            .ThrowsAsync(new JsonSerializationException("bad wishlist"));
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(wishlistService: wishlistService, logger: logger);

        var result = await controller.CheckWithlistItem(1);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
        VerifyLogged(logger, LogLevel.Warning);
    }

    [Test]
    public async Task RerunScrapeHistoryAsync_BypassesCacheAndRecordsNewHistory()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        db.AutomatedScrapeHistories.Add(new AutomatedScrapeHistory
        {
            UserId = TestDb.TestUserId,
            Endpoint = "scrape-page",
            ScrapeType = "Web Scrape",
            GameUrlId = 1,
            Page = 2,
            SetupJson = "{}",
            ResultCount = 1,
            Date = DateTime.UtcNow,
            IsHaveError = false
        });
        db.SaveChanges();

        using var cache = TestDb.CreateMemoryCache();
        cache.Set(
            string.Format(CacheKeys.ScrapePage, 1, 2),
            new[] { new WatchItemDto { Name = "Cached", Price = 1 } });

        var response = new[] { new WatchItemDto { Name = "Fresh Rerun", Price = 4 } };
        var steamService = new Mock<ISteamService>();
        steamService.Setup(x => x.ScrapePage(1, 2)).ReturnsAsync(response);
        var controller = CreateController(steamService: steamService, cache: cache, database: database);
        var originalId = db.AutomatedScrapeHistories.Single().Id;

        var result = await controller.RerunScrapeHistoryAsync(originalId);

        var ok = result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(db.AutomatedScrapeHistories.Count(), Is.EqualTo(2));
            Assert.That(db.AutomatedScrapeHistories.OrderBy(x => x.Id).Last().ResultsJson, Does.Contain("Fresh Rerun"));
        });
        steamService.Verify(x => x.ScrapePage(1, 2), Times.Once);
    }

    [Test]
    public async Task GetScrapeHistoryAsync_ReturnsOnlyCurrentUserRows()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        db.AutomatedScrapeHistories.AddRange(
            new AutomatedScrapeHistory
            {
                UserId = TestDb.TestUserId,
                Endpoint = "scrape-page",
                ScrapeType = "Web Scrape",
                GameUrlId = 1,
                Page = 1,
                SetupJson = "{}",
                Date = DateTime.UtcNow,
                IsHaveError = false
            },
            new AutomatedScrapeHistory
            {
                UserId = "other-user",
                Endpoint = "scrape-page",
                ScrapeType = "Web Scrape",
                GameUrlId = 1,
                Page = 1,
                SetupJson = "{}",
                Date = DateTime.UtcNow,
                IsHaveError = false
            });
        db.SaveChanges();
        var otherId = db.AutomatedScrapeHistories.Single(x => x.UserId == "other-user").Id;
        var controller = CreateController(database: database);

        var listResult = await controller.GetScrapeHistoryAsync();
        var detailResult = await controller.GetScrapeHistoryDetailAsync(otherId);

        var ok = listResult as OkObjectResult;
        var items = ok?.Value as IEnumerable<SteamApp.Application.DTOs.ScrapeHistory.ScrapeHistorySummaryDto>;
        Assert.Multiple(() =>
        {
            Assert.That(items?.Count(), Is.EqualTo(1));
            Assert.That(items?.Single().GameUrlName, Is.EqualTo("Batch URL"));
            Assert.That(detailResult, Is.TypeOf<NotFoundResult>());
        });
    }

    [Test]
    public async Task ScrapeHistoryEndpoints_HandleParallelReadsAndRerunsWithSeparateContexts()
    {
        using var database = TestDb.CreateSeededDatabase();
        var db = database.Context;
        db.AutomatedScrapeHistories.Add(new AutomatedScrapeHistory
        {
            UserId = TestDb.TestUserId,
            Endpoint = "scrape-page",
            ScrapeType = "Web Scrape",
            GameUrlId = 1,
            Page = 2,
            SetupJson = "{}",
            ResultCount = 1,
            Date = DateTime.UtcNow,
            IsHaveError = false
        });
        db.SaveChanges();

        var originalId = db.AutomatedScrapeHistories.Single().Id;
        var steamService = new Mock<ISteamService>();
        steamService
            .Setup(x => x.ScrapePage(1, 2))
            .ReturnsAsync(() => [new WatchItemDto { Name = "Parallel Rerun", Price = 5 }]);

        using var cache = TestDb.CreateMemoryCache();
        var controller = CreateController(
            steamService: steamService,
            cache: cache,
            database: database);

        var tasks = Enumerable.Range(0, 15)
            .Select(index => (index % 3) switch
            {
                0 => controller.GetScrapeHistoryAsync(),
                1 => controller.GetScrapeHistoryDetailAsync(originalId),
                _ => controller.RerunScrapeHistoryAsync(originalId)
            })
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.All.TypeOf<OkObjectResult>());
            Assert.That(db.AutomatedScrapeHistories.Count(), Is.EqualTo(6));
            steamService.Verify(x => x.ScrapePage(1, 2), Times.Exactly(5));
        });
    }

    private static SteamController CreateController(
        Mock<ISteamService>? steamService = null,
        Mock<IWishlistService>? wishlistService = null,
        Mock<ILogger<SteamController>>? logger = null,
        IMemoryCache? cache = null,
        TestDatabase? database = null)
    {
        var dbFactory = database?.Factory ?? TestDb.CreateSeededFactory();
        var scrapeHistoryData = new ScrapeHistoryDataService(dbFactory);

        var controller = new SteamController(
            (steamService ?? new Mock<ISteamService>()).Object,
            (wishlistService ?? new Mock<IWishlistService>()).Object,
            (logger ?? new Mock<ILogger<SteamController>>()).Object,
            dbFactory,
            scrapeHistoryData,
            cache ?? TestDb.CreateMemoryCache());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, TestDb.TestUserId)],
                    "Test"))
            }
        };

        return controller;
    }

    private static void VerifyLogged<T>(Mock<ILogger<T>> logger, LogLevel level)
    {
        logger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
