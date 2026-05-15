using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.Services;
using SteamApp.Interfaces.Services;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Controllers;

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
        cache.Set(string.Format(CacheKeys.WishListItem, 10), cached);

        var wishlistService = new Mock<IWishlistService>(MockBehavior.Strict);
        var controller = CreateController(wishlistService: wishlistService, cache: cache);

        var result = await controller.CheckWithlistItem(10);

        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.SameAs(cached));
        wishlistService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CheckWishlistItem_ReturnsBadRequestForJsonSerializationException()
    {
        var wishlistService = new Mock<IWishlistService>();
        wishlistService
            .Setup(x => x.CheckWishlistItem(10))
            .ThrowsAsync(new JsonSerializationException("bad wishlist"));
        var logger = new Mock<ILogger<SteamController>>();
        var controller = CreateController(wishlistService: wishlistService, logger: logger);

        var result = await controller.CheckWithlistItem(10);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
        VerifyLogged(logger, LogLevel.Warning);
    }

    private static SteamController CreateController(
        Mock<ISteamService>? steamService = null,
        Mock<IWishlistService>? wishlistService = null,
        Mock<ILogger<SteamController>>? logger = null,
        IMemoryCache? cache = null)
    {
        return new SteamController(
            (steamService ?? new Mock<ISteamService>()).Object,
            (wishlistService ?? new Mock<IWishlistService>()).Object,
            (logger ?? new Mock<ILogger<SteamController>>()).Object,
            cache ?? TestDb.CreateMemoryCache());
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
