using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Services;

namespace SteamApp.IntegrationTests.Jobs;

[TestFixture]
public sealed class WishlistJobIntegrationTests
{
    private const string WishlistCheckQueue = "test.wishlist.check";
    private const string WishlistNotificationQueue = "test.wishlist.notification";

    [Test]
    public async Task WishlistCheckJobPublishesCheckRequestForActiveRecipient()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
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

        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.Single().WishlistId, Is.EqualTo(1));
            Assert.That(messages.Single().WishlistName, Is.EqualTo("Active Wish"));
            Assert.That(messages.Single().Email, Is.EqualTo("owner@example.com"));
            Assert.That(messages.Single().CorrelationId, Is.Not.Empty);
            Assert.That(
                cache.TryGetValue(
                    string.Format(CacheKeys.WishListBackgroundJobQueued, 1),
                    out _),
                Is.True);
        });
    }

    [Test]
    public async Task WishlistCheckJobSkipsWhenNoRecipientsExist()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var publisher = new CapturingMessagePublisher();
        var job = CreateJob(cache, publisher, RecipientService());

        await job.RunAsync(CancellationToken.None);

        Assert.That(publisher.Messages, Is.Empty);
    }

    [Test]
    public async Task WishlistCheckJobSkipsCachedNotification()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            new WhishListResponse
            {
                GameName = "Cached Game",
                CurrentPrice = 1,
                IsPriceReached = true
            });
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
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(
            string.Format(CacheKeys.WishListBackgroundJobQueued, 1),
            true);
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
        using var cache = new MemoryCache(new MemoryCacheOptions());
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
        using var cache = new MemoryCache(new MemoryCacheOptions());
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
    public async Task WishlistNotificationHandlerSendsEmailAndSetsCache()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var email = new CapturingEmailService();
        var handler = CreateNotificationHandler(cache, email);

        await handler.HandleAsync(
            NotificationMessage(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(email.Messages, Has.Count.EqualTo(1));
            Assert.That(email.Messages.Single().To, Is.EqualTo("owner@example.com"));
            Assert.That(email.Messages.Single().Subject, Does.Contain("Active Game"));
            Assert.That(
                cache.TryGetValue(
                    string.Format(CacheKeys.WishListBackgroundJob, 1),
                    out WhishListResponse? cached),
                Is.True);
            Assert.That(cached?.GameName, Is.EqualTo("Active Game"));
            Assert.That(cached?.CurrentPrice, Is.EqualTo(4.5));
        });
    }

    [Test]
    public async Task WishlistNotificationHandlerSkipsDuplicateWhenCacheExists()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            new WhishListResponse
            {
                GameName = "Cached Game",
                CurrentPrice = 1,
                IsPriceReached = true
            });
        var email = new CapturingEmailService();
        var handler = CreateNotificationHandler(cache, email);

        await handler.HandleAsync(
            NotificationMessage(),
            CancellationToken.None);

        Assert.That(email.Messages, Is.Empty);
    }

    private static WishlistCheckJob CreateJob(
        IMemoryCache cache,
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
        IMemoryCache cache,
        CapturingMessagePublisher publisher,
        FakeWishlistService wishlist)
    {
        return new WishlistCheckMessageHandler(
            NullLogger<WishlistCheckMessageHandler>.Instance,
            Options.Create(CreateRabbitMqOptions()),
            cache,
            wishlist,
            publisher);
    }

    private static WishlistNotificationMessageHandler CreateNotificationHandler(
        IMemoryCache cache,
        CapturingEmailService email)
    {
        return new WishlistNotificationMessageHandler(
            NullLogger<WishlistNotificationMessageHandler>.Instance,
            cache,
            email);
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
            "Active Wish",
            "owner@example.com",
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
    }
}
