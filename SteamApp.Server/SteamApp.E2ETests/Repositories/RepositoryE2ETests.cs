using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.E2ETests.Repositories;

[TestFixture]
public sealed class RepositoryE2ETests
{
    [Test]
    public async Task ApiFlowsPersistRelationshipsThroughEfAndExposeProjectedNames()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var productTags = await client.GetAsync("/api/product-tags/");
        var gameUrlPixels = await client.GetAsync("/api/game-url-pixels/");
        var deleteReferencedGame = await client.DeleteAsync("/api/games/1");

        var productTagsJson = await productTags.ReadJsonElementAsync();
        var gameUrlPixelsJson = await gameUrlPixels.ReadJsonElementAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ProductTags.Add(new ProductTags { ProductId = 1, TagId = 1 });

        var duplicateJoinException = Assert.CatchAsync<DbUpdateException>(
            async () => await db.SaveChangesAsync());

        Assert.Multiple(() =>
        {
            Assert.That(productTags.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(gameUrlPixels.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteReferencedGame.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(productTagsJson[0].GetProperty("productName").GetString(), Is.EqualTo("Rocket Launcher"));
            Assert.That(productTagsJson[0].GetProperty("tagName").GetString(), Is.EqualTo("Primary"));
            Assert.That(gameUrlPixelsJson[0].GetProperty("pixelName").GetString(), Is.EqualTo("Team Spirit"));
            Assert.That(duplicateJoinException, Is.Not.Null);
        });
    }
}
