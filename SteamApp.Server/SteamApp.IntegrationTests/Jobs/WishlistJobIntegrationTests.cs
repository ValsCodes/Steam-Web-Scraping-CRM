using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.IntegrationTests.Support;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Services;

namespace SteamApp.IntegrationTests.Jobs;

[TestFixture]
public sealed class WishlistJobIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string WishlistCheckQueue = "test.wishlist.check";
    private const string WishlistNotificationQueue = "test.wishlist.notification";

    [Test]
    public async Task WishlistCheckJobPublishesCheckRequestForActiveRecipient()
    {
        var cache = CreateCache();
        var publisher = new CapturingMessagePublisher();
        var job = CreateJob(
            cache,
            publisher,
            RecipientService(new WishlistNotificationRecipient(
                1,
                "Active Wish",
                "owner@example.com")));

        await job.RunAsync(CancellationToken.None);

        var messages = publisher.GetMessages<WishlistCheckRequested>(WishlistCheckQueue);
        var queuedMarker = await cache.GetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJobQueued, 1));

        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.Single().WishlistId, Is.EqualTo(1));
            Assert.That(messages.Single().CorrelationId, Is.Not.Empty);
            Assert.That(queuedMarker, Is.Not.Null);
        });
    }

    [Test]
    public async Task WishlistCheckJobSkipsWhenNoRecipientsExist()
    {
        var cache = CreateCache();
        var publisher = new CapturingMessagePublisher();
        var job = CreateJob(cache, publisher, RecipientService());

        await job.RunAsync(CancellationToken.None);

        Assert.That(publisher.Messages, Is.Empty);
    }

    [Test]
    public async Task WishlistCheckJobSkipsCachedNotification()
    {
        var cache = CreateCache();
        await cache.SetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            JsonSerializer.Serialize(new WhishListResponse
            {
                GameName = "Cached Game",
                CurrentPrice = 1,
                IsPriceReached = true
            },
            JsonOptions));
        var publisher = new CapturingMessagePublisher();
        var job = CreateJob(
            cache,
            publisher,
            RecipientService(new WishlistNotificationRecipient(
                1,
                "Active Wish",
                "owner@example.com")));

        await job.RunAsync(CancellationToken.None);

        Assert.That(publisher.Messages, Is.Empty);
    }

    [Test]
    public async Task WishlistCheckJobSkipsQueuedWishlistItem()
    {
        var cache = CreateCache();
        await cache.SetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJobQueued, 1),
            "1");
        var publisher = new CapturingMessagePublisher();
        var job = CreateJob(
            cache,
            publisher,
            RecipientService(new WishlistNotificationRecipient(
                1,
                "Active Wish",
                "owner@example.com")));

        await job.RunAsync(CancellationToken.None);

        Assert.That(publisher.Messages, Is.Empty);
    }

    [Test]
    public async Task WishlistCheckHandlerPublishesNotificationWhenPriceIsReached()
    {
        var cache = CreateCache();
        var publisher = new CapturingMessagePublisher();
        var wishlist = new FakeWishlistService();
        var handler = CreateCheckHandler(cache, publisher, wishlist);

        await handler.HandleAsync(
            CheckMessage(),
            CancellationToken.None);

        var messages = publisher.GetMessages<WishlistNotificationRequested>(WishlistNotificationQueue);

        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.Single().WishlistId, Is.EqualTo(1));
            Assert.That(messages.Single().Email, Is.EqualTo("owner@example.com"));
            Assert.That(messages.Single().GameName, Is.EqualTo("Active Game"));
            Assert.That(messages.Single().CurrentPrice, Is.EqualTo(4.5));
            Assert.That(messages.Single().CorrelationId, Is.EqualTo("correlation-1"));
            Assert.That(wishlist.CheckCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task WishlistCheckHandlerDoesNotPublishNotificationWhenPriceIsNotReached()
    {
        var cache = CreateCache();
        var publisher = new CapturingMessagePublisher();
        var wishlist = new FakeWishlistService();
        wishlist.SetResponse(1, new WhishListResponse
        {
            GameName = "Active Game",
            CurrentPrice = 6,
            IsPriceReached = false
        });
        var handler = CreateCheckHandler(cache, publisher, wishlist);

        await handler.HandleAsync(
            CheckMessage(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(publisher.Messages, Is.Empty);
            Assert.That(wishlist.CheckCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task WishlistCheckHandlerSkipsCachedNotificationWithoutRunningCheck()
    {
        var cache = CreateCache();
        await cache.SetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            "cached");
        var publisher = new CapturingMessagePublisher();
        var wishlist = new FakeWishlistService();
        var handler = CreateCheckHandler(cache, publisher, wishlist);

        await handler.HandleAsync(CheckMessage(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(publisher.Messages, Is.Empty);
            Assert.That(wishlist.CheckCalls, Is.Zero);
        });
    }

    [Test]
    public async Task WishlistCheckHandlerSkipsDeletedOrInactiveAlertWithoutRunningCheck()
    {
        var cache = CreateCache();
        var publisher = new CapturingMessagePublisher();
        var wishlist = new FakeWishlistService();
        var handler = CreateCheckHandler(cache, publisher, wishlist, hasRecipient: false);

        await handler.HandleAsync(CheckMessage(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(publisher.Messages, Is.Empty);
            Assert.That(wishlist.CheckCalls, Is.Zero);
        });
    }

    [Test]
    public async Task WishlistNotificationHandlerSendsEmailAndSetsCache()
    {
        var cache = CreateCache();
        var email = new CapturingEmailService();
        var handler = CreateNotificationHandler(cache, email);

        await handler.HandleAsync(
            NotificationMessage(),
            CancellationToken.None);

        var cachedJson = await cache.GetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJob, 1));
        var cached = JsonSerializer.Deserialize<WhishListResponse>(cachedJson!, JsonOptions);

        Assert.Multiple(() =>
        {
            Assert.That(email.Messages, Has.Count.EqualTo(1));
            Assert.That(email.Messages.Single().To, Is.EqualTo("owner@example.com"));
            Assert.That(email.Messages.Single().Subject, Does.Contain("Active Game"));
            Assert.That(cachedJson, Is.Not.Null);
            Assert.That(cached?.GameName, Is.EqualTo("Active Game"));
            Assert.That(cached?.CurrentPrice, Is.EqualTo(4.5));
        });
    }

    [Test]
    public async Task WishlistNotificationHandlerSkipsDuplicateWhenCacheExists()
    {
        var cache = CreateCache();
        await cache.SetStringAsync(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            JsonSerializer.Serialize(new WhishListResponse
            {
                GameName = "Cached Game",
                CurrentPrice = 1,
                IsPriceReached = true
            },
            JsonOptions));
        var email = new CapturingEmailService();
        var handler = CreateNotificationHandler(cache, email);

        await handler.HandleAsync(
            NotificationMessage(),
            CancellationToken.None);

        Assert.That(email.Messages, Is.Empty);
    }

    private static WishlistCheckJob CreateJob(
        IDistributedCache cache,
        CapturingMessagePublisher publisher,
        IWishlistNotificationRecipientService recipientService)
    {
        return new WishlistCheckJob(
            NullLogger<WishlistCheckJob>.Instance,
            Options.Create(CreateRabbitMqOptions()),
            cache,
            publisher,
            recipientService);
    }

    private static WishlistCheckMessageHandler CreateCheckHandler(
        IDistributedCache cache,
        CapturingMessagePublisher publisher,
        FakeWishlistService wishlist,
        bool hasRecipient = true)
    {
        var execution = new Mock<IWishlistCheckExecutionService>();
        execution
            .Setup(x => x.ExecuteScheduledAsync(
                It.IsAny<long>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (
                long id,
                DateTime requestedAtUtc,
                string correlationId,
                CancellationToken cancellationToken) =>
            {
                var checkResult = await wishlist.CheckWishlistItem(id, cancellationToken);
                if (checkResult.IsFailure)
                {
                    return Result<WishlistCheckExecutionOutcome>.Failure(checkResult.Error!);
                }

                var response = checkResult.Value!;
                var trace = new WishListCheckHistoryDto
                {
                    Id = 1,
                    WishListId = id,
                    GameName = response.GameName,
                    Source = "Scheduled",
                    Status = "Succeeded",
                    CurrentPrice = response.CurrentPrice,
                    IsPriceReached = response.IsPriceReached,
                    RequestedAtUtc = requestedAtUtc,
                    StartedAtUtc = requestedAtUtc,
                    CompletedAtUtc = requestedAtUtc,
                    CorrelationId = correlationId
                };

                return Result<WishlistCheckExecutionOutcome>.Success(
                    new WishlistCheckExecutionOutcome(trace, CheckError: null));
            });

        return new WishlistCheckMessageHandler(
            NullLogger<WishlistCheckMessageHandler>.Instance,
            Options.Create(CreateRabbitMqOptions()),
            cache,
            execution.Object,
            hasRecipient
                ? RecipientService(new WishlistNotificationRecipient(
                    1,
                    "Active Wish",
                    "owner@example.com"))
                : RecipientService(),
            publisher);
    }

    private static WishlistNotificationMessageHandler CreateNotificationHandler(
        IDistributedCache cache,
        CapturingEmailService email)
    {
        return new WishlistNotificationMessageHandler(
            NullLogger<WishlistNotificationMessageHandler>.Instance,
            cache,
            email);
    }

    private static MemoryDistributedCache CreateCache()
    {
        return new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions()));
    }

    private static RabbitMqOptions CreateRabbitMqOptions()
    {
        return new RabbitMqOptions
        {
            WishlistCheckQueueName = WishlistCheckQueue,
            WishlistNotificationQueueName = WishlistNotificationQueue,
            WishlistCheckDelay = TimeSpan.Zero
        };
    }

    private static WishlistCheckRequested CheckMessage()
    {
        return new WishlistCheckRequested(
            1,
            DateTime.UtcNow,
            "correlation-1");
    }

    private static WishlistNotificationRequested NotificationMessage()
    {
        return new WishlistNotificationRequested(
            1,
            "Active Wish",
            "owner@example.com",
            "Active Game",
            4.5,
            DateTime.UtcNow,
            "correlation-1");
    }

    private static IWishlistNotificationRecipientService RecipientService(
        params WishlistNotificationRecipient[] recipients)
    {
        return new StubRecipientService(recipients);
    }

    private sealed class StubRecipientService(
        IReadOnlyList<WishlistNotificationRecipient> recipients)
        : IWishlistNotificationRecipientService
    {
        public Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(recipients);
        }

        public Task<WishlistNotificationRecipient?> GetActiveRecipientAsync(
            long wishlistId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(recipients.SingleOrDefault(
                recipient => recipient.WishlistId == wishlistId));
        }
    }
}
