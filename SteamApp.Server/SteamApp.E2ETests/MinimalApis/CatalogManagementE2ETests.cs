using System.Net;
using System.Net.Http.Json;
using SteamApp.E2ETests.Support;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.E2ETests.MinimalApis;

[TestFixture]
public sealed class CatalogManagementE2ETests
{
    [Test]
    public async Task UserCanCreateRelateReadUpdateAndDeleteCatalogRecords()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();
        await client.AuthenticateWithClientCredentialsAsync();

        var gameCreate = await client.PostAsJsonAsync("/api/games/", new
        {
            name = "E2E Game",
            baseUrl = "https://steam.example/e2e",
            pageUrl = "https://steam.example/app/e2e",
            internalId = 9001,
            isActive = true
        });
        var createdGame = await gameCreate.ReadRequiredJsonAsync();
        var gameId = createdGame.GetProperty("id").GetInt64();

        var productCreate = await client.PostAsJsonAsync("/api/products/", new
        {
            gameId,
            name = "E2E Product",
            rating = 4,
            isActive = true
        });
        var product = await productCreate.ReadRequiredJsonAsync();
        var productId = product.GetProperty("id").GetInt64();

        var tagCreate = await client.PostAsJsonAsync("/api/tags/", new
        {
            gameId,
            name = "E2E Tag",
            isActive = true
        });
        var tag = await tagCreate.ReadRequiredJsonAsync();
        var tagId = tag.GetProperty("id").GetInt64();

        var relationCreate = await client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId,
            tagId
        });
        var duplicateRelation = await client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId,
            tagId
        });
        var fetchedRelation = await client.GetAsync($"/api/product-tags/{productId}/{tagId}");

        var gamePatch = await client.PatchAsJsonAsync($"/api/games/{gameId}", new
        {
            id = gameId,
            isActive = false
        });
        var deleteBlocked = await client.DeleteAsync("/api/games/1");
        var deleteRelation = await client.DeleteAsync($"/api/product-tags/{productId}/{tagId}");
        var missingRelation = await client.GetAsync($"/api/product-tags/{productId}/{tagId}");

        Assert.Multiple(() =>
        {
            Assert.That(gameCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(productCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(tagCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(relationCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(duplicateRelation.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(fetchedRelation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(gamePatch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteBlocked.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteRelation.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(missingRelation.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task PagedCatalogEndpointsFilterSortAndClampForAUserJourney()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();
        await client.AuthenticateWithClientCredentialsAsync();

        var clamped = await client.GetAsync("/api/games/paged?pageNumber=99&pageSize=1");
        var filtered = await client.GetAsync("/api/products/paged?gameId=1&name=Rocket&minRating=4&tagIds=1&pageSize=10");
        var sorted = await client.GetAsync("/api/pixels/paged?pageSize=2&sortBy=name&sortDirection=desc");

        var clampedPage = await clamped.ReadRequiredJsonAsync();
        var filteredPage = await filtered.ReadRequiredJsonAsync();
        var sortedPage = await sorted.ReadRequiredJsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(clampedPage.GetProperty("pageNumber").GetInt32(), Is.EqualTo(3));
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
}
