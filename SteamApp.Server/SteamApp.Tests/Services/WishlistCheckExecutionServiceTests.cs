using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Interfaces.Services;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Contracts.Pagination;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class WishlistCheckExecutionServiceTests
{
    private static readonly DateTimeOffset TestNow = new(
        2026,
        10,
        2,
        12,
        0,
        0,
        TimeSpan.Zero);

    [Test]
    public async Task ExecuteManualAsync_Success_PersistsCompletedTrace()
    {
        using var database = TestDb.CreateSeededDatabase();
        var wishlist = new Mock<IWishlistService>();
        wishlist
            .Setup(x => x.CheckWishlistItem(1, CancellationToken.None))
            .ReturnsAsync(Result<WhishListResponse>.Success(new WhishListResponse
            {
                GameName = "Alpha Game",
                CurrentPrice = 8.5,
                IsPriceReached = true
            }));
        var service = CreateService(database, wishlist.Object);

        var result = await service.ExecuteManualAsync(
            1,
            TestDb.TestUserId,
            "trace-1",
            CancellationToken.None);

        var history = database.Context.WishListCheckHistories.AsNoTracking().Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.CheckError, Is.Null);
            Assert.That(history.Source, Is.EqualTo(WishListCheckSourceEnum.Manual));
            Assert.That(history.Status, Is.EqualTo(WishListCheckStatusEnum.Succeeded));
            Assert.That(history.TargetPrice, Is.EqualTo(9.99));
            Assert.That(history.CurrentPrice, Is.EqualTo(8.5));
            Assert.That(history.IsPriceReached, Is.True);
            Assert.That(history.CorrelationId, Is.EqualTo("trace-1"));
            Assert.That(history.UserId, Is.EqualTo(TestDb.TestUserId));
        });
    }

    [Test]
    public async Task ExecuteScheduledAsync_ExpectedFailure_PersistsRedactedFailure()
    {
        using var database = TestDb.CreateSeededDatabase();
        var wishlist = new Mock<IWishlistService>();
        wishlist
            .Setup(x => x.CheckWishlistItem(1, CancellationToken.None))
            .ReturnsAsync(Result<WhishListResponse>.Failure(new Error(
                "WishlistCheck.PriceUnavailable",
                "Steam did not provide a readable price for this game.",
                ErrorType.Unavailable)));
        var service = CreateService(database, wishlist.Object);

        var result = await service.ExecuteScheduledAsync(
            1,
            TestNow.UtcDateTime.AddSeconds(-10),
            "scheduled-trace",
            CancellationToken.None);

        var history = database.Context.WishListCheckHistories.AsNoTracking().Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.CheckError?.Type, Is.EqualTo(ErrorType.Unavailable));
            Assert.That(history.Source, Is.EqualTo(WishListCheckSourceEnum.Scheduled));
            Assert.That(history.Status, Is.EqualTo(WishListCheckStatusEnum.Failed));
            Assert.That(history.ErrorCode, Is.EqualTo("WishlistCheck.PriceUnavailable"));
            Assert.That(history.ErrorText, Is.EqualTo("Steam did not provide a readable price for this game."));
            Assert.That(history.ErrorText, Does.Not.Contain("stack"));
        });
    }

    [Test]
    public async Task ExecuteManualAsync_Canceled_PersistsCanceledTraceAndPropagatesCancellation()
    {
        using var database = TestDb.CreateSeededDatabase();
        using var cancellation = new CancellationTokenSource();
        var wishlist = new Mock<IWishlistService>();
        wishlist
            .Setup(x => x.CheckWishlistItem(1, cancellation.Token))
            .Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromCanceled<Result<WhishListResponse>>(cancellation.Token);
            });
        var service = CreateService(database, wishlist.Object);

        Assert.That(
            async () => await service.ExecuteManualAsync(
                1,
                TestDb.TestUserId,
                "canceled-trace",
                cancellation.Token),
            Throws.TypeOf<TaskCanceledException>());

        var history = database.Context.WishListCheckHistories.AsNoTracking().Single();
        Assert.That(history.Status, Is.EqualTo(WishListCheckStatusEnum.Canceled));
    }

    [Test]
    public async Task ExecuteManualAsync_OtherOwner_ReturnsNotFoundWithoutHistory()
    {
        using var database = TestDb.CreateSeededDatabase();
        var wishlist = new Mock<IWishlistService>(MockBehavior.Strict);
        var service = CreateService(database, wishlist.Object);

        var result = await service.ExecuteManualAsync(
            1,
            "other-user",
            "trace",
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.NotFound));
            Assert.That(database.Context.WishListCheckHistories, Is.Empty);
        });
        wishlist.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetHistoryAsync_ReturnsNewestOwnedPageAndCapsPageSize()
    {
        using var database = TestDb.CreateSeededDatabase();
        for (var index = 0; index < 105; index++)
        {
            database.Context.WishListCheckHistories.Add(new WishListCheckHistory
            {
                WishListId = 1,
                UserId = TestDb.TestUserId,
                GameName = "Alpha Game",
                Source = WishListCheckSourceEnum.Manual,
                Status = WishListCheckStatusEnum.Succeeded,
                RequestedAtUtc = TestNow.UtcDateTime.AddMinutes(index),
                StartedAtUtc = TestNow.UtcDateTime.AddMinutes(index),
                CompletedAtUtc = TestNow.UtcDateTime.AddMinutes(index).AddSeconds(1),
                CorrelationId = $"trace-{index}"
            });
        }
        await database.Context.SaveChangesAsync();
        var service = CreateService(database, Mock.Of<IWishlistService>());

        var result = await service.GetHistoryAsync(
            1,
            TestDb.TestUserId,
            new WishListCheckHistoryPageQuery { PageNumber = 1, PageSize = 1000 },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value?.Items, Has.Count.EqualTo(100));
            Assert.That(result.Value?.PageSize, Is.EqualTo(100));
            Assert.That(result.Value?.TotalCount, Is.EqualTo(105));
            Assert.That(result.Value?.Items.First().CorrelationId, Is.EqualTo("trace-104"));
        });
    }

    [Test]
    public void WishListHistoryRelationship_DeletesHistoryWithAlert()
    {
        using var database = TestDb.CreateSeededDatabase();
        var relationship = database.Context.Model
            .FindEntityType(typeof(WishListCheckHistory))!
            .GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WishList));

        Assert.That(relationship.DeleteBehavior, Is.EqualTo(DeleteBehavior.Cascade));
    }

    private static WishlistCheckExecutionService CreateService(
        TestDatabase database,
        IWishlistService wishlistService)
    {
        return new WishlistCheckExecutionService(
            database.Factory,
            wishlistService,
            new FixedTimeProvider(TestNow),
            NullLogger<WishlistCheckExecutionService>.Instance);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
