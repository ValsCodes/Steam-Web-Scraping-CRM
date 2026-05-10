using System.Net;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.Controllers;

[TestFixture]
public sealed class SteamControllerIntegrationTests
{
    [Test]
    public async Task ScrapePageEndpointRequiresAuthAndCachesSuccessfulResult()
    {
        using var factory = new SteamAppFactory();
        using var anonymous = factory.CreateAnonymousClient();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var unauthorized = await anonymous.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var first = await client.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var second = await client.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var firstJson = await first.ReadJsonElementAsync();
        var secondJson = await second.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(firstJson.GetArrayLength(), Is.EqualTo(1));
            Assert.That(secondJson.GetArrayLength(), Is.EqualTo(1));
            Assert.That(factory.SteamService.ScrapePageCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ScrapePublicApiEndpointReturnsMappedFakeSteamData()
    {
        using var factory = new SteamAppFactory();
        factory.SteamService.PublicApiResults =
        [
            new SteamApp.Application.DTOs.WatchItem.WatchItemDto
            {
                Name = "Public A",
                Price = 1.25,
                Quantity = 4
            }
        ];
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/steam/scrape-public-api/gameUrl/1/page/1");
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json[0].GetProperty("name").GetString(), Is.EqualTo("Public A"));
            Assert.That(json[0].GetProperty("price").GetDouble(), Is.EqualTo(1.25).Within(0.001));
        });
    }

    [Test]
    public async Task ScrapePixelsEndpointReturnsBadRequestForInvalidListing()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();
        factory.SteamService.ThrowInvalidPixelListing = true;

        var response = await client.GetAsync("/steam/scrape-pixels/gameUrl/1/page/1");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Contain("Invalid Listing"));
        });
    }

    [Test]
    public async Task WishlistCheckEndpointCachesPriceReachedResult()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var first = await client.GetAsync("/steam/check-wishlist/1");
        var second = await client.GetAsync("/steam/check-wishlist/1");
        var json = await first.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.GetProperty("isPriceReached").GetBoolean(), Is.True);
            Assert.That(json.GetProperty("gameName").GetString(), Is.EqualTo("Active Game"));
            Assert.That(factory.WishlistService.CheckCalls, Is.EqualTo(1));
        });
    }
}
