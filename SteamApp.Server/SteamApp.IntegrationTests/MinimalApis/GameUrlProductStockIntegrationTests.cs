using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.MinimalApis;

[TestFixture]
public sealed class GameUrlProductStockIntegrationTests
{
    [Test]
    public async Task StockEndpoints_AssignAdjustAndList_PersistAuditedValues()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var initial = await client.GetFromJsonAsync<JsonElement>("/api/game-url-products/1");
        var assigned = await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = 50 });
        var incremented = await client.PatchAsync(
            "/api/game-url-products/1/1/current-stock/increment",
            null);
        var decremented = await client.PatchAsync(
            "/api/game-url-products/1/1/current-stock/decrement",
            null);
        var negative = await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = -4 });

        var assignedBody = await assigned.Content.ReadFromJsonAsync<JsonElement>();
        var incrementedBody = await incremented.Content.ReadFromJsonAsync<JsonElement>();
        var decrementedBody = await decremented.Content.ReadFromJsonAsync<JsonElement>();
        var negativeBody = await negative.Content.ReadFromJsonAsync<JsonElement>();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var relation = await db.GameUrlsProducts.FindAsync(1L, 1L);
        var history = db.GameUrlProductStockHistories.OrderBy(x => x.Id).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(initial[0].GetProperty("currentStock").GetInt32(), Is.Zero);
            Assert.That(assigned.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(assignedBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(50));
            Assert.That(incrementedBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(51));
            Assert.That(decrementedBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(50));
            Assert.That(negativeBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(-4));
            Assert.That(relation?.CurrentStock, Is.EqualTo(-4));
            Assert.That(history.Select(x => x.Operation), Is.EqualTo(new[]
            {
                GameUrlProductStockOperationEnum.Assigned,
                GameUrlProductStockOperationEnum.Incremented,
                GameUrlProductStockOperationEnum.Decremented,
                GameUrlProductStockOperationEnum.Assigned,
            }));
            Assert.That(history.Select(x => (x.PreviousStock, x.NewStock)), Is.EqualTo(new[]
            {
                (0, 50), (50, 51), (51, 50), (50, -4),
            }));
            Assert.That(history.All(x => x.UserId == IntegrationSeed.UserId), Is.True);
            Assert.That(history.All(x => x.CreatedAtUtc > DateTime.MinValue), Is.True);
        });
    }

    [Test]
    public async Task StockEndpoints_SameAndClampedValues_DoNotCreateHistory()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = int.MaxValue });
        var clampedMaximum = await client.PatchAsync(
            "/api/game-url-products/1/1/current-stock/increment",
            null);
        await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = int.MaxValue });
        await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = int.MinValue });
        var clampedMinimum = await client.PatchAsync(
            "/api/game-url-products/1/1/current-stock/decrement",
            null);

        var maximumBody = await clampedMaximum.Content.ReadFromJsonAsync<JsonElement>();
        var minimumBody = await clampedMinimum.Content.ReadFromJsonAsync<JsonElement>();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Multiple(() =>
        {
            Assert.That(maximumBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(int.MaxValue));
            Assert.That(minimumBody.GetProperty("currentStock").GetInt32(), Is.EqualTo(int.MinValue));
            Assert.That(db.GameUrlProductStockHistories.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task StockHistory_ReturnsNewestPagedResultsAndCapsPageSize()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            for (var index = 1; index <= 30; index++)
            {
                db.GameUrlProductStockHistories.Add(new GameUrlProductStockHistory
                {
                    ProductId = 1,
                    GameUrlId = 1,
                    PreviousStock = index - 1,
                    NewStock = index,
                    Operation = GameUrlProductStockOperationEnum.Incremented,
                    CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(index),
                    UserId = IntegrationSeed.UserId,
                });
            }
            await db.SaveChangesAsync();
        }

        var pageResponse = await client.GetAsync(
            "/api/game-url-products/1/1/current-stock/history?page=2&pageSize=25");
        var cappedResponse = await client.GetAsync(
            "/api/game-url-products/1/1/current-stock/history?page=1&pageSize=500");
        var page = await pageResponse.Content.ReadFromJsonAsync<JsonElement>();
        var capped = await cappedResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(page.GetProperty("pageNumber").GetInt32(), Is.EqualTo(2));
            Assert.That(page.GetProperty("items").GetArrayLength(), Is.EqualTo(5));
            Assert.That(page.GetProperty("items")[0].GetProperty("newStock").GetInt32(), Is.EqualTo(5));
            Assert.That(page.GetProperty("totalCount").GetInt32(), Is.EqualTo(30));
            Assert.That(capped.GetProperty("pageSize").GetInt32(), Is.EqualTo(100));
            Assert.That(capped.GetProperty("items")[0].GetProperty("newStock").GetInt32(), Is.EqualTo(30));
        });
    }

    [Test]
    public async Task StockEndpoints_UnauthorizedOrOtherUserRelation_DoNotExposeOrMutateStock()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        using var anonymousClient = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.Add(new ApplicationUser
            {
                Id = "other-user",
                UserName = "other-user",
                NormalizedUserName = "OTHER-USER",
                Email = "other@example.test",
                NormalizedEmail = "OTHER@EXAMPLE.TEST",
                SecurityStamp = "other-user-security-stamp",
            });
            db.Games.Add(new Game { Id = 99, Name = "Other", UserId = "other-user" });
            db.Products.Add(new Product { Id = 99, GameId = 99, Name = "Other", UserId = "other-user" });
            db.GameUrls.Add(new GameUrl
            {
                Id = 99,
                GameId = 99,
                PartialUrl = "https://example.test/{0}",
                UserId = "other-user",
            });
            db.GameUrlsProducts.Add(new GameUrlProducts { ProductId = 99, GameUrlId = 99 });
            await db.SaveChangesAsync();
        }

        var anonymous = await anonymousClient.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = 7 });
        var foreignUpdate = await client.PutAsJsonAsync(
            "/api/game-url-products/99/99/current-stock",
            new { currentStock = 7 });
        var foreignHistory = await client.GetAsync(
            "/api/game-url-products/99/99/current-stock/history");

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var foreignRelation = await verifyDb.GameUrlsProducts.FindAsync(99L, 99L);

        Assert.Multiple(() =>
        {
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(foreignUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(foreignHistory.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(foreignRelation?.CurrentStock, Is.Zero);
            Assert.That(verifyDb.GameUrlProductStockHistories.Any(x => x.ProductId == 99), Is.False);
        });
    }

    [Test]
    public async Task DeleteRelation_WithStockHistory_RetainsAuditRows()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        await client.PutAsJsonAsync(
            "/api/game-url-products/1/1/current-stock",
            new { currentStock = 10 });
        var deleted = await client.DeleteAsync("/api/game-url-products/1/1");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(db.GameUrlsProducts.Any(x => x.ProductId == 1 && x.GameUrlId == 1), Is.False);
            Assert.That(db.GameUrlProductStockHistories.Count(x => x.ProductId == 1 && x.GameUrlId == 1), Is.EqualTo(1));
        });
    }
}
