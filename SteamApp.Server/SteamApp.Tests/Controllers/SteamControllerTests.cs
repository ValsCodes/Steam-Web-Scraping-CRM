using Wire = SteamApp.Contracts.Enums;
using SteamApp.Application.Features.Scraping;
using SteamApp.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Contracts.ScrapeHistory;
using SteamApp.Contracts.WatchItem;
using SteamApp.Contracts.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Persistence;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Endpoints.Controllers;
using SteamApp.Application.Abstractions.Messaging;
using SteamApp.Infrastructure.Messaging.Handlers.Scraping;
using SteamApp.Contracts.Events.Scraping;
using SteamApp.Infrastructure.Messaging.RabbitMq.Options;
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
        steamService.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(response);
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
        steamService.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(response);
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
        steamService.Setup(x => x.ScrapeFromPublicApi(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(response);
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
            .Setup(x => x.ScrapeWithPixels(1, 2, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.ScrapeWithPixels(1, 2, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.ScrapeWithPixels(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(steamService: steamService, logger: logger);

        var result = await controller.ScrapeForPixelsAsync(1, 2);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        VerifyLogged(logger, LogLevel.Error);
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
        steamService.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(response);
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
        steamService.Verify(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>()), Times.Once);
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
        var items = ok?.Value as IEnumerable<SteamApp.Contracts.ScrapeHistory.ScrapeHistorySummaryDto>;
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
            .Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>()))
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
            steamService.Verify(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>()), Times.Exactly(5));
        });
    }

    [Test]
    public async Task QueueScrapePageAsync_CreatesQueuedHistoryAndPublishesMessage()
    {
        using var database = TestDb.CreateSeededDatabase();
        var messagePublisher = new Mock<IMessagePublisher>();
        ScrapeRequested? published = null;
        string? queueName = null;
        var options = new RabbitMqOptions
        {
            Enabled = true,
            ScrapeRequestQueueName = "test.scrape.requests"
        };

        messagePublisher
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<ScrapeRequested>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, ScrapeRequested, CancellationToken>((queue, message, _) =>
            {
                queueName = queue;
                published = message;
            })
            .Returns(Task.CompletedTask);

        var controller = CreateController(
            messagePublisher: messagePublisher,
            rabbitMqOptions: options,
            database: database);

        var result = await controller.QueueScrapePageAsync(1, 2);

        var accepted = result as AcceptedResult;
        var dto = accepted?.Value as ScrapeJobAcceptedDto;
        var history = database.Context.AutomatedScrapeHistories.Single();

        Assert.Multiple(() =>
        {
            Assert.That(accepted, Is.Not.Null);
            Assert.That(dto?.HistoryId, Is.EqualTo(history.Id));
            Assert.That(dto?.Status, Is.EqualTo(Wire.ScrapeJobStatusEnum.Queued));
            Assert.That(queueName, Is.EqualTo("test.scrape.requests"));
            Assert.That(published?.HistoryId, Is.EqualTo(history.Id));
            Assert.That(published?.Endpoint, Is.EqualTo("scrape-page"));
            Assert.That(history.Status, Is.EqualTo(ScrapeJobStatusEnum.Queued));
            Assert.That(history.CorrelationId, Is.Not.Empty);
            Assert.That(history.ResultsJson, Is.Null);
        });
    }

    [Test]
    public async Task QueueScrapePageAsync_BrokerUnavailable_LeavesDurableJobForRecoveryWithoutCache()
    {
        using var database = TestDb.CreateSeededDatabase();
        var publisher = new Mock<IMessagePublisher>();
        publisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<ScrapeRequested>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Broker unavailable"));
        var controller = CreateController(messagePublisher: publisher, database: database);

        var response = await controller.QueueScrapePageAsync(1, 2);
        var history = database.Context.AutomatedScrapeHistories.AsNoTracking().Single();
        Assert.That(response, Is.TypeOf<AcceptedResult>());
        Assert.That(history.Status, Is.EqualTo(ScrapeJobStatusEnum.Queued));

        var cache = new Mock<IDistributedCache>(MockBehavior.Strict);
        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Cache unavailable"));
        cache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Cache unavailable"));
        var steam = new Mock<ISteamService>();
        steam.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new WatchItemDto { Name = "Recovered" } });
        await CreateScrapeHandler(database, steam, cache.Object).RecoverAsync(history.Id, CancellationToken.None);

        var recovered = database.Context.AutomatedScrapeHistories.AsNoTracking().Single();
        Assert.Multiple(() =>
        {
            Assert.That(recovered.Status, Is.EqualTo(ScrapeJobStatusEnum.Succeeded));
            Assert.That(recovered.ResultsJson, Does.Contain("Recovered"));
        });
    }

    [Test]
    public async Task ScrapeRequestedHandler_RejectsMismatchedMessagesAndSkipsActiveDuplicate()
    {
        using var database = TestDb.CreateSeededDatabase();
        var data = new ScrapeHistoryDataService(database.Factory);
        var url = await data.GetOwnedGameUrlSnapshotAsync(1, TestDb.TestUserId, CancellationToken.None);
        var history = await data.CreateQueuedHistoryAsync(TestDb.TestUserId, url!, 2,
            "scrape-page", "Web Scrape", "correlation", CancellationToken.None);
        var message = CreateScrapeMessage(history);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var steam = new Mock<ISteamService>(MockBehavior.Strict);
        steam.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).Returns(async () =>
        {
            started.TrySetResult();
            await finish.Task;
            return Array.Empty<WatchItemDto>();
        });
        var handler = CreateScrapeHandler(database, steam);
        await handler.HandleAsync(message with { GameUrlId = 2 }, CancellationToken.None);
        await handler.HandleAsync(message with { UserId = "other-user" }, CancellationToken.None);
        await handler.HandleAsync(message with { CorrelationId = "other-job" }, CancellationToken.None);
        Assert.That((await data.GetJobAsync(history.Id, CancellationToken.None))!.Status,
            Is.EqualTo(ScrapeJobStatusEnum.Queued));

        var first = handler.HandleAsync(message, CancellationToken.None);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await handler.HandleAsync(message, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            finish.TrySetResult();
            await first;
        }

        steam.Verify(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task QueueScrapePageAsync_ReturnsServiceUnavailableWhenRabbitMqDisabled()
    {
        using var database = TestDb.CreateSeededDatabase();
        var messagePublisher = new Mock<IMessagePublisher>(MockBehavior.Strict);
        var controller = CreateController(
            messagePublisher: messagePublisher,
            rabbitMqOptions: new RabbitMqOptions { Enabled = false },
            database: database);

        var result = await controller.QueueScrapePageAsync(1, 2);

        var objectResult = result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
            Assert.That(database.Context.AutomatedScrapeHistories, Is.Empty);
        });
        messagePublisher.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapeRequestedHandler_StoppingCancelsOutboundWorkAndLeavesJobRecoverable()
    {
        using var database = TestDb.CreateSeededDatabase();
        var data = new ScrapeHistoryDataService(database.Factory);
        var url = await data.GetOwnedGameUrlSnapshotAsync(1, TestDb.TestUserId, CancellationToken.None);
        var history = await data.CreateQueuedHistoryAsync(TestDb.TestUserId, url!, 2,
            "scrape-page", "Web Scrape", "correlation", CancellationToken.None);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var steam = new Mock<ISteamService>(MockBehavior.Strict);
        steam.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>()))
            .Returns(async (long _, short _, CancellationToken token) =>
            {
                started.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return Array.Empty<WatchItemDto>();
            });
        using var stopping = new CancellationTokenSource();
        var processing = CreateScrapeHandler(database, steam).HandleAsync(CreateScrapeMessage(history), stopping.Token);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            await stopping.CancelAsync();
        }

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await processing.WaitAsync(TimeSpan.FromSeconds(5)));
        var future = DateTime.UtcNow.AddMinutes(6);
        Assert.That(await data.GetRecoverableJobIdsAsync(future, future, CancellationToken.None), Does.Contain(history.Id));
    }

    [Test]
    public async Task ScrapeRequestedHandler_MarksSucceededWithFreshResults()
    {
        using var database = TestDb.CreateSeededDatabase();
        var scrapeHistoryData = new ScrapeHistoryDataService(database.Factory);
        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            1,
            TestDb.TestUserId,
            CancellationToken.None);
        var history = await scrapeHistoryData.CreateQueuedHistoryAsync(
            TestDb.TestUserId,
            gameUrl!,
            2,
            "scrape-page",
            "Web Scrape",
            "correlation",
            CancellationToken.None);
        var response = new[] { new WatchItemDto { Name = "Async Fresh", Price = 4 } };
        var steamService = new Mock<ISteamService>();
        steamService.Setup(x => x.ScrapePage(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var cache = TestDb.CreateDistributedCache();
        var handler = CreateScrapeHandler(database, steamService, cache);

        await handler.HandleAsync(CreateScrapeMessage(history), CancellationToken.None);

        var updated = database.Context.AutomatedScrapeHistories.AsNoTracking().Single(x => x.Id == history.Id);
        var cachedResult = await cache.GetAsync(string.Format(CacheKeys.ScrapePage, 1, 2));
        Assert.Multiple(() =>
        {
            Assert.That(updated.Status, Is.EqualTo(ScrapeJobStatusEnum.Succeeded));
            Assert.That(updated.IsHaveError, Is.False);
            Assert.That(updated.ResultsJson, Does.Contain("Async Fresh"));
            Assert.That(updated.StartedAtUtc, Is.Not.Null);
            Assert.That(updated.CompletedAtUtc, Is.Not.Null);
            Assert.That(cachedResult, Is.Not.Null);
        });
    }

    [Test]
    public async Task ScrapeRequestedHandler_MarksFailedWithMappedError()
    {
        using var database = TestDb.CreateSeededDatabase();
        var scrapeHistoryData = new ScrapeHistoryDataService(database.Factory);
        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            2,
            TestDb.TestUserId,
            CancellationToken.None);
        var history = await scrapeHistoryData.CreateQueuedHistoryAsync(
            TestDb.TestUserId,
            gameUrl!,
            2,
            "scrape-pixels",
            "Pixel Scrape",
            "correlation",
            CancellationToken.None);
        var steamService = new Mock<ISteamService>();
        steamService
            .Setup(x => x.ScrapeWithPixels(2, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new JsonSerializationException("bad listing"));
        var handler = CreateScrapeHandler(database, steamService);

        await handler.HandleAsync(CreateScrapeMessage(history), CancellationToken.None);

        var updated = database.Context.AutomatedScrapeHistories.AsNoTracking().Single(x => x.Id == history.Id);
        Assert.Multiple(() =>
        {
            Assert.That(updated.Status, Is.EqualTo(ScrapeJobStatusEnum.Failed));
            Assert.That(updated.IsHaveError, Is.True);
            Assert.That(updated.ErrorText, Is.EqualTo("Error: Invalid Listing"));
            Assert.That(updated.ResultsJson, Is.Null);
        });
    }

    [Test]
    public async Task ScrapeRequestedHandler_CompletesFromCacheWithoutCallingSteamService()
    {
        using var database = TestDb.CreateSeededDatabase();
        var scrapeHistoryData = new ScrapeHistoryDataService(database.Factory);
        var gameUrl = await scrapeHistoryData.GetOwnedGameUrlSnapshotAsync(
            1,
            TestDb.TestUserId,
            CancellationToken.None);
        var history = await scrapeHistoryData.CreateQueuedHistoryAsync(
            TestDb.TestUserId,
            gameUrl!,
            2,
            "scrape-page",
            "Web Scrape",
            "correlation",
            CancellationToken.None);
        var cache = TestDb.CreateDistributedCache();
        await cache.SetStringAsync(
            string.Format(CacheKeys.ScrapePage, 1, 2),
            System.Text.Json.JsonSerializer.Serialize(
                new[] { new WatchItemDto { Name = "Cached Async", Price = 2 } }));
        var steamService = new Mock<ISteamService>(MockBehavior.Strict);
        var handler = CreateScrapeHandler(database, steamService, cache);

        await handler.HandleAsync(CreateScrapeMessage(history), CancellationToken.None);

        var updated = database.Context.AutomatedScrapeHistories.AsNoTracking().Single(x => x.Id == history.Id);
        Assert.Multiple(() =>
        {
            Assert.That(updated.Status, Is.EqualTo(ScrapeJobStatusEnum.Succeeded));
            Assert.That(updated.ResultsJson, Does.Contain("Cached Async"));
        });
        steamService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapeRequestedHandler_SkipsTerminalHistoryRows()
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
            ResultsJson = "[]",
            ResultCount = 0,
            Date = DateTime.UtcNow,
            IsHaveError = false,
            Status = ScrapeJobStatusEnum.Succeeded
        });
        db.SaveChanges();
        var history = db.AutomatedScrapeHistories.Single();
        var steamService = new Mock<ISteamService>(MockBehavior.Strict);
        var handler = CreateScrapeHandler(database, steamService);

        await handler.HandleAsync(CreateScrapeMessage(history), CancellationToken.None);

        steamService.VerifyNoOtherCalls();
    }

    private static SteamController CreateController(
        Mock<ISteamService>? steamService = null,
        Mock<ILogger<SteamController>>? logger = null,
        Mock<IMessagePublisher>? messagePublisher = null,
        RabbitMqOptions? rabbitMqOptions = null,
        IMemoryCache? cache = null,
        TestDatabase? database = null)
    {
        var dbFactory = database?.Factory ?? TestDb.CreateSeededFactory();
        var scrapeHistoryData = new ScrapeHistoryDataService(dbFactory);
        var steamServiceInstance = (steamService ?? new Mock<ISteamService>()).Object;

        var controller = new SteamController(
            (logger ?? new Mock<ILogger<SteamController>>()).Object,
            scrapeHistoryData,
            new ScrapeExecutionService(steamServiceInstance),
            (messagePublisher ?? new Mock<IMessagePublisher>()).Object,
            Options.Create(rabbitMqOptions ?? new RabbitMqOptions { Enabled = true }),
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

    private static ScrapeRequestedMessageHandler CreateScrapeHandler(
        TestDatabase database,
        Mock<ISteamService> steamService,
        IDistributedCache? cache = null)
    {
        return new ScrapeRequestedMessageHandler(
            Mock.Of<ILogger<ScrapeRequestedMessageHandler>>(),
            cache ?? TestDb.CreateDistributedCache(),
            new ScrapeExecutionService(steamService.Object),
            new ScrapeHistoryDataService(database.Factory));
    }

    private static ScrapeRequested CreateScrapeMessage(AutomatedScrapeHistory history)
    {
        return new ScrapeRequested(
            history.Id,
            history.UserId ?? TestDb.TestUserId,
            history.GameUrlId,
            history.Page,
            history.Endpoint,
            history.ScrapeType,
            history.Date,
            history.CorrelationId ?? "correlation");
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
