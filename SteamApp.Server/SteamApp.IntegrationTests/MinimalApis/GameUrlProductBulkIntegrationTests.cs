using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Application.DTOs.GameUrlProduct;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.MinimalApis;

[TestFixture]
public sealed class GameUrlProductBulkIntegrationTests
{
    [Test]
    public async Task BulkSync_LargeSelectionAndRetry_PersistsEveryRelationAndPreservesStock()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();
        var productIds = Enumerable.Range(10, 699).Select(id => (long)id).Prepend(1L).ToArray();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Products.AddRange(productIds.Skip(1).Select(id => new Product
            {
                Id = id, GameId = 1, Name = $"Product {id}", IsActive = true, UserId = IntegrationSeed.UserId,
            }));
            (await db.GameUrlsProducts.FindAsync(1L, 1L))!.CurrentStock = 42;
            await db.SaveChangesAsync();
        }

        var first = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = productIds });
        var retry = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = productIds.Concat(new long[] { 1, 10 }).ToArray() });
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var relations = await verify.GameUrlsProducts.Where(x => x.GameUrlId == 1).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(retry.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(relations.Select(x => x.ProductId), Is.EquivalentTo(productIds));
            Assert.That(relations.Single(x => x.ProductId == 1).CurrentStock, Is.EqualTo(42));
        });
    }

    [Test]
    public async Task BulkSync_InsertFailureAfterDelete_RollsBackAllEarlierWrites()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Products.AddRange(
                new Product { Id = 10, GameId = 1, Name = "Ten", UserId = IntegrationSeed.UserId },
                new Product { Id = 11, GameId = 1, Name = "Eleven", UserId = IntegrationSeed.UserId });
            (await db.GameUrlsProducts.FindAsync(1L, 1L))!.CurrentStock = 42;
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER reject_bulk_insert BEFORE INSERT ON game_url_products " +
                "WHEN NEW.product_id = 11 BEGIN SELECT RAISE(ABORT, 'Forced bulk failure'); END;");
        }

        var response = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [10, 11] });
        var errorBody = await response.Content.ReadAsStringAsync();
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var relations = await verify.GameUrlsProducts.Where(x => x.GameUrlId == 1).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(errorBody, Does.Not.Contain("Forced bulk failure"));
            Assert.That(relations.Select(x => x.ProductId), Is.EqualTo(new long[] { 1 }));
            Assert.That(relations[0].CurrentStock, Is.EqualTo(42));
        });
    }

    [TestCase(null)]
    [TestCase(new long[] { 0 })]
    [TestCase(new long[] { -1 })]
    [TestCase(new long[] { 1, 404 })]
    [TestCase(new long[] { 1, 2 })]
    public async Task BulkSync_InvalidOrWrongGameIds_LeavesExistingSelectionUnchanged(long[]? productIds)
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = productIds });
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(db.GameUrlsProducts.Where(x => x.GameUrlId == 1).Select(x => x.ProductId),
                Is.EqualTo(new long[] { 1 }));
        });
    }

    [Test]
    public async Task BulkSync_OversizedSelection_ReturnsBadRequestWithoutMutation()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = Enumerable.Repeat(1L, 10_001).ToArray() });
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(db.GameUrlsProducts.Count(x => x.GameUrlId == 1), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task BulkSync_EmptySelection_RemovesOnlyThatUrlsRelations()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.GameUrlsProducts.Add(new GameUrlProducts { ProductId = 1, GameUrlId = 2 });
            await db.SaveChangesAsync();
        }

        var response = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [] });
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(verify.GameUrlsProducts.Any(x => x.GameUrlId == 1), Is.False);
            Assert.That(verify.GameUrlsProducts.Any(x => x.GameUrlId == 2 && x.ProductId == 1), Is.True);
        });
    }

    [Test]
    public async Task BulkSync_AnonymousOrForeignIds_RejectsWithoutMutating()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        using var anonymous = factory.CreateAnonymousClient();
        using var wrongScope = factory.CreateAuthenticatedClient("unrelated-scope");
        await factory.ResetDatabaseAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.Add(new ApplicationUser { Id = "other-user", UserName = "other-user" });
            db.Products.Add(new Product { Id = 99, GameId = 1, Name = "Foreign", UserId = "other-user" });
            db.GameUrls.Add(new GameUrl { Id = 99, GameId = 1, Name = "Foreign", UserId = "other-user" });
            await db.SaveChangesAsync();
        }

        var unauthenticated = await anonymous.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [] });
        var foreignProduct = await client.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [1, 99] });
        var foreignUrl = await client.PutAsJsonAsync("/api/game-url-products/99/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [1] });
        var forbidden = await wrongScope.PutAsJsonAsync("/api/game-url-products/1/bulk",
            new GameUrlProductBulkSyncDto { ProductIds = [] });
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(unauthenticated.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(foreignProduct.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(foreignUrl.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(verify.GameUrlsProducts.Where(x => x.GameUrlId == 1).Select(x => x.ProductId),
                Is.EqualTo(new long[] { 1 }));
        });
    }
}
