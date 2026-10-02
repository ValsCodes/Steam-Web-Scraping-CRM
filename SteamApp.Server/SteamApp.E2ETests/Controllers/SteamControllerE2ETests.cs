using System.Net;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.E2ETests.Controllers;

[TestFixture]
public sealed class SteamControllerE2ETests
{
    [Test]
    public async Task SteamScrapeEndpointsRequireAuthReturnMappedResultsAndUseCache()
    {
        using var factory = new SteamAppFactory();
        using var anonymous = factory.CreateAnonymousClient();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var anonymousResponse = await anonymous.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var firstPage = await client.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var secondPage = await client.GetAsync("/steam/scrape-page/gameUrl/1/page/1");
        var publicApi = await client.GetAsync("/steam/scrape-public-api/gameUrl/1/page/1");

        factory.SteamService.ThrowInvalidPixelListing = true;
        var invalidPixelListing = await client.GetAsync("/steam/scrape-pixels/gameUrl/1/page/1");

        var pageJson = await firstPage.ReadJsonElementAsync();
        var publicJson = await publicApi.ReadJsonElementAsync();
        var invalidPixelBody = await invalidPixelListing.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(anonymousResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(firstPage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondPage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(publicApi.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(invalidPixelListing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(pageJson[0].GetProperty("name").GetString(), Is.EqualTo("Batch Item"));
            Assert.That(publicJson[0].GetProperty("name").GetString(), Is.EqualTo("Public Item"));
            Assert.That(invalidPixelBody, Does.Contain("Invalid Listing"));
            Assert.That(factory.SteamService.ScrapePageCalls, Is.EqualTo(1));
            Assert.That(factory.SteamService.ScrapeFromPublicApiCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task WishlistCheckEndpointReturnsPriceReachedStateAndStoresHistory()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var first = await client.PostAsync("/api/wish-list/1/checks", null);
        var second = await client.PostAsync("/api/wish-list/1/checks", null);
        var history = await client.GetAsync("/api/wish-list/1/checks?pageSize=25");
        var json = await first.ReadJsonElementAsync();
        var historyJson = await history.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(history.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.GetProperty("isPriceReached").GetBoolean(), Is.True);
            Assert.That(json.GetProperty("gameName").GetString(), Is.EqualTo("Active Game"));
            Assert.That(historyJson.GetProperty("totalCount").GetInt32(), Is.EqualTo(2));
            Assert.That(factory.WishlistService.CheckCalls, Is.EqualTo(2));
        });
    }
}
