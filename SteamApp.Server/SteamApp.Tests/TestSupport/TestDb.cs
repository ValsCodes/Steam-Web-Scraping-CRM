using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;

namespace SteamApp.Tests.TestSupport;

public static class TestDb
{
    public const string TestUserId = "test-user";
    private static readonly InMemoryDatabaseRoot SharedRoot = new();

    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        return CreateContext(CreateOptions(databaseName));
    }

    public static TestDatabase CreateDatabase()
    {
        var options = CreateOptions(root: SharedRoot);
        var db = CreateContext(options);

        return new TestDatabase(db, new TestDbContextFactory(options));
    }

    public static TestDatabase CreateSeededDatabase()
    {
        var database = CreateDatabase();
        SeedBaseline(database.Context);

        return database;
    }

    public static TestDbContextFactory CreateSeededFactory()
    {
        using var database = CreateSeededDatabase();

        return database.Factory;
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions(
        string? databaseName = null,
        InMemoryDatabaseRoot? root = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var name = databaseName ?? Guid.NewGuid().ToString("N");

        if (root is null)
        {
            builder.UseInMemoryDatabase(name);
        }
        else
        {
            builder.UseInMemoryDatabase(name, root);
        }

        return builder.Options;
    }

    private static ApplicationDbContext CreateContext(DbContextOptions<ApplicationDbContext> options)
    {
        var db = new ApplicationDbContext(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    public static MemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    public static void SeedBaseline(ApplicationDbContext db)
    {
        db.Games.AddRange(
            new Game
            {
                Id = 1,
                Name = "Alpha Game",
                BaseUrl = "https://steam.example/alpha",
                PageUrl = "https://steam.example/app/1",
                InternalId = 10,
                IsActive = true,
                UserId = TestUserId
            },
            new Game
            {
                Id = 2,
                Name = "Beta Game",
                BaseUrl = "https://steam.example/beta",
                PageUrl = "https://steam.example/app/2",
                InternalId = 20,
                IsActive = false,
                UserId = TestUserId
            });

        db.GameUrls.AddRange(
            new GameUrl
            {
                Id = 1,
                Name = "Batch URL",
                GameId = 1,
                ScrapingModeId = (long)ScrapingModeEnum.Batch,
                PartialUrl = "https://steam.example/search?p={0}",
                StartPage = 1,
                EndPage = 5,
                IsActive = true,
                UserId = TestUserId
            },
            new GameUrl
            {
                Id = 2,
                Name = "Pixel URL",
                GameId = 1,
                ScrapingModeId = (long)ScrapingModeEnum.PixelBatch,
                PartialUrl = "https://steam.example/pixels?p={0}",
                PixelX = 4,
                PixelY = 5,
                PixelImageWidth = 62,
                PixelImageHeight = 62,
                IsActive = true,
                UserId = TestUserId
            });

        db.Products.AddRange(
            new Product
            {
                Id = 1,
                GameId = 1,
                Name = "Rocket Launcher",
                Rating = 5,
                IsActive = true,
                UserId = TestUserId
            },
            new Product
            {
                Id = 2,
                GameId = 2,
                Name = "Medigun",
                Rating = 3,
                IsActive = false,
                UserId = TestUserId
            });

        db.Pixels.AddRange(
            new Pixel
            {
                Id = 1,
                GameId = 1,
                Name = "Team Spirit",
                RedValue = 195,
                GreenValue = 108,
                BlueValue = 45,
                IsActive = true,
                UserId = TestUserId
            },
            new Pixel
            {
                Id = 2,
                GameId = 1,
                Name = "After Eight",
                RedValue = 45,
                GreenValue = 48,
                BlueValue = 44,
                IsActive = false,
                UserId = TestUserId
            });

        db.Tags.AddRange(
            new Tag { Id = 1, GameId = 1, Name = "Primary", IsActive = true, UserId = TestUserId },
            new Tag { Id = 2, GameId = 2, Name = "Support", IsActive = false, UserId = TestUserId });

        db.WishLists.AddRange(
            new WishList
            {
                Id = 1,
                GameId = 1,
                Name = "Cheap Alpha",
                Price = 9.99,
                IsActive = true,
                UserId = TestUserId
            },
            new WishList
            {
                Id = 2,
                GameId = 2,
                Name = "Cheap Beta",
                Price = 4.99,
                IsActive = false,
                UserId = TestUserId
            });

        db.WatchList.AddRange(
            new WatchList
            {
                Id = 1,
                Name = "Watch Alpha",
                Url = "https://steam.example/watch/1",
                RegistrationDate = new DateOnly(2026, 1, 1),
                IsActive = true,
                UserId = TestUserId
            },
            new WatchList
            {
                Id = 2,
                Name = "Watch Beta",
                Url = "https://steam.example/watch/2",
                RegistrationDate = new DateOnly(2026, 2, 1),
                IsActive = false,
                UserId = TestUserId
            });

        db.FeedbackRequests.AddRange(
            new FeedbackRequest
            {
                Id = 1,
                Type = FeedbackRequestTypeEnum.Feedback,
                Title = "Improve filters",
                Description = "Please make the search filters easier to scan.",
                Area = "Catalog",
                Status = FeedbackRequestStatusEnum.Active,
                CreatedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                StatusChangedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UserId = TestUserId
            },
            new FeedbackRequest
            {
                Id = 2,
                Type = FeedbackRequestTypeEnum.Bug,
                Title = "Export bug",
                Description = "The export button should keep the current filters.",
                Area = "Exports",
                Status = FeedbackRequestStatusEnum.Processed,
                CreatedAtUtc = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                StatusChangedAtUtc = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UserId = TestUserId
            });

        db.ProductTags.Add(new ProductTags { ProductId = 1, TagId = 1 });
        db.GameUrlsProducts.Add(new GameUrlProducts { ProductId = 1, GameUrlId = 1 });
        db.GameUrlsPixels.Add(new GameUrlPixels { PixelId = 1, GameUrlId = 2 });

        db.SaveChanges();
    }
}

public sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(options);
    }
}

public sealed class TestDatabase(
    ApplicationDbContext context,
    TestDbContextFactory factory)
    : IDisposable
{
    public ApplicationDbContext Context { get; } = context;
    public TestDbContextFactory Factory { get; } = factory;

    public void Dispose()
    {
        Context.Dispose();
    }
}
