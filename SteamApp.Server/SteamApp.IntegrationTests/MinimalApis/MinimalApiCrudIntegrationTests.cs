using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.Game;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Application.DTOs.Pixel;
using SteamApp.Application.DTOs.Product;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.MinimalApis;

[TestFixture]
public sealed class MinimalApiCrudIntegrationTests
{
    [TestCase("/api/games/")]
    [TestCase("/api/game-urls/")]
    [TestCase("/api/scraping-modes/")]
    [TestCase("/api/products/")]
    [TestCase("/api/pixels/")]
    [TestCase("/api/watch-list/")]
    [TestCase("/api/wish-list/")]
    [TestCase("/api/game-url-products/")]
    [TestCase("/api/tags/")]
    [TestCase("/api/product-tags/")]
    [TestCase("/api/game-url-pixels/")]
    public async Task GetAllEndpointsReturnSeededData(string path)
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync(path);
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(json.GetArrayLength(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task GamesCrudFlowPersistsDataAndInvalidatesCache()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var create = await client.PostAsJsonAsync("/api/games/", new GameCreateDto
        {
            Name = "Created Game",
            BaseUrl = "https://steam.example/created",
            PageUrl = "https://steam.example/app/99",
            InternalId = 99,
            IsActive = true
        });

        using (var scope = factory.Services.CreateScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            cache.Set(string.Format(CacheKeys.Game, 1), new Game { Id = 1, Name = "Cached" });
        }

        var update = await client.PutAsJsonAsync("/api/games/1", new GameUpdateDto
        {
            Id = 1,
            Name = "Updated Alpha",
            BaseUrl = "https://steam.example/updated",
            PageUrl = "https://steam.example/app/updated",
            InternalId = 101,
            IsActive = false
        });
        var patch = await client.PatchAsJsonAsync("/api/games/1", new
        {
            id = 1,
            isActive = true
        });
        var deleteBlocked = await client.DeleteAsync("/api/games/1");
        var deleteAllowed = await client.DeleteAsync("/api/games/3");

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cacheAfter = verifyScope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var updated = await db.Games.FindAsync(1L);
        var createdExists = db.Games.Any(x => x.Name == "Created Game");

        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(create.Headers.Location?.ToString(), Does.StartWith("/api/games/"));
            Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(patch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteBlocked.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteAllowed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(updated?.Name, Is.EqualTo("Updated Alpha"));
            Assert.That(updated?.IsActive, Is.True);
            Assert.That(createdExists, Is.True);
            Assert.That(db.Games.Find(3L), Is.Null);
            Assert.That(cacheAfter.TryGetValue(string.Format(CacheKeys.Game, 1), out _), Is.False);
        });
    }

    [Test]
    public async Task PagedEndpointsFilterSortAndClampPagination()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var clamped = await client.GetAsync("/api/games/paged?pageNumber=99&pageSize=1");
        var filtered = await client.GetAsync("/api/products/paged?gameId=1&name=Rocket&minRating=4&tagIds=1&pageSize=10");
        var sorted = await client.GetAsync("/api/pixels/paged?pageSize=2&sortBy=name&sortDirection=desc");

        var clampedPage = await clamped.ReadJsonElementAsync();
        var filteredPage = await filtered.ReadJsonElementAsync();
        var sortedPage = await sorted.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(clamped.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(clampedPage.GetProperty("pageNumber").GetInt32(), Is.EqualTo(3));
            Assert.That(clampedPage.GetProperty("pageSize").GetInt32(), Is.EqualTo(1));
            Assert.That(clampedPage.GetProperty("totalCount").GetInt32(), Is.EqualTo(3));
            Assert.That(clampedPage.GetProperty("totalPages").GetInt32(), Is.EqualTo(3));

            Assert.That(filteredPage.GetProperty("totalCount").GetInt32(), Is.EqualTo(1));
            Assert.That(
                filteredPage.GetProperty("items")[0].GetProperty("name").GetString(),
                Is.EqualTo("Rocket Launcher"));

            Assert.That(
                sortedPage.GetProperty("items")[0].GetProperty("name").GetString(),
                Is.EqualTo("Team Spirit"));
        });
    }

    [Test]
    public async Task ForeignKeyValidationReturnsBadRequest()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var invalidWish = await client.PostAsJsonAsync("/api/wish-list/", new WishListCreateDto
        {
            GameId = 404,
            Name = "Invalid",
            Price = 1,
            IsActive = true
        });
        var invalidPixel = await client.PostAsJsonAsync("/api/pixels/", new PixelCreateDto
        {
            GameId = 404,
            Name = "Invalid",
            RedValue = 1,
            GreenValue = 2,
            BlueValue = 3,
            IsActive = true
        });
        var invalidGameUrl = await client.PostAsJsonAsync("/api/game-urls/", new GameUrlCreateDto
        {
            GameId = 1,
            ScrapingModeId = 404,
            PartialUrl = "https://steam.example/{0}",
            IsActive = true
        });

        Assert.Multiple(() =>
        {
            Assert.That(invalidWish.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidPixel.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidGameUrl.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task ProductPixelGameUrlAndWishListStatusFlowsWork()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var productCreate = await client.PostAsJsonAsync("/api/products/", new ProductCreateDto
        {
            GameId = 1,
            Name = "New Product",
            Rating = 4,
            IsActive = true
        });
        var productDuplicate = await client.PostAsJsonAsync("/api/products/", new ProductCreateDto
        {
            GameId = 1,
            Name = " rocket launcher ",
            Rating = 4,
            IsActive = true
        });
        var pixelPatch = await client.PatchAsJsonAsync("/api/pixels/2", new
        {
            id = 2,
            isActive = true
        });
        var gameUrlPatch = await client.PatchAsJsonAsync("/api/game-urls/1", new
        {
            id = 1,
            isActive = false
        });
        var wishPatch = await client.PatchAsJsonAsync("/api/wish-list/1", new
        {
            id = 1,
            isActive = false
        });

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Multiple(() =>
        {
            Assert.That(productCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(productDuplicate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(pixelPatch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(gameUrlPatch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(wishPatch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(db.Pixels.Find(2L)?.IsActive, Is.True);
            Assert.That(db.GameUrls.Find(1L)?.IsActive, Is.False);
            Assert.That(db.WishLists.Find(1L)?.IsActive, Is.False);
        });
    }

    [Test]
    public async Task CompositeJoinEndpointsCreateRejectDuplicateFetchAndDelete()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var duplicateProductTag = await client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId = 1,
            tagId = 1
        });
        var createdProductTag = await client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId = 2,
            tagId = 2
        });
        var fetchedProductTag = await client.GetAsync("/api/product-tags/2/2");
        var deletedProductTag = await client.DeleteAsync("/api/product-tags/2/2");
        var missingProductTag = await client.GetAsync("/api/product-tags/2/2");

        var duplicateGameUrlProduct = await client.PostAsJsonAsync("/api/game-url-products/", new
        {
            productId = 1,
            gameUrlId = 1
        });
        var createdGameUrlProduct = await client.PostAsJsonAsync("/api/game-url-products/", new
        {
            productId = 2,
            gameUrlId = 2
        });
        var duplicateGameUrlPixel = await client.PostAsJsonAsync("/api/game-url-pixels/", new
        {
            pixelId = 1,
            gameUrlId = 2
        });
        var createdGameUrlPixel = await client.PostAsJsonAsync("/api/game-url-pixels/", new
        {
            pixelId = 2,
            gameUrlId = 2
        });

        Assert.Multiple(() =>
        {
            Assert.That(duplicateProductTag.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(createdProductTag.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(fetchedProductTag.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deletedProductTag.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(missingProductTag.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(duplicateGameUrlProduct.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(createdGameUrlProduct.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(duplicateGameUrlPixel.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(createdGameUrlPixel.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        });
    }

    [Test]
    public async Task MissingResourcesReturnNotFound()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var getMissing = await client.GetAsync("/api/watch-list/404");
        var putMissing = await client.PutAsJsonAsync("/api/games/404", new GameUpdateDto
        {
            Name = "Missing"
        });
        var deleteMissing = await client.DeleteAsync("/api/products/404");

        Assert.Multiple(() =>
        {
            Assert.That(getMissing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(putMissing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(deleteMissing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }
}
