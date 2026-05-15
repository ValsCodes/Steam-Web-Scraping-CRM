using System.Net;
using System.Net.Http.Json;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.Contracts;

[TestFixture]
public sealed class ApiContractIntegrationTests
{
    [Test]
    public async Task SwaggerDocumentContainsBearerSecuritySchemeAndKnownEndpoints()
    {
        using var factory = new SteamAppFactory(environmentName: "Development");
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _), Is.True);
            Assert.That(json.GetProperty("paths").TryGetProperty("/api/games", out _), Is.True);
            Assert.That(json.GetProperty("paths").TryGetProperty("/steam/scrape-page/gameUrl/{gamerUrlId}/page/{page}", out _), Is.True);
        });
    }

    [Test]
    public async Task JsonResponsesUseCamelCase()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/api/games/1");
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.TryGetProperty("baseUrl", out _), Is.True);
            Assert.That(json.TryGetProperty("BaseUrl", out _), Is.False);
        });
    }

    [Test]
    public async Task ValidationErrorsReturnProblemDetailsShape()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "duplicate@example.com",
            password = "weak"
        });
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(json.TryGetProperty("errors", out _), Is.True);
        });
    }
}
