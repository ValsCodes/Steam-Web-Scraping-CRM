using System.Net;
using System.Net.Http.Json;
using SteamApp.E2ETests.Support;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Security;

namespace SteamApp.E2ETests.Security;

[TestFixture]
public sealed class SecurityE2ETests
{
    [Test]
    public async Task ClientCredentialsTokenUnlocksProtectedApiSession()
    {
        using var factory = new SteamAppFactory();
        using var anonymous = factory.CreateAnonymousClient();
        using var client = factory.CreateAnonymousClient();
        using var wrongScope = factory.CreateAuthenticatedClient("wrong-scope");
        await factory.ResetDatabaseAsync();

        var anonymousGames = await anonymous.GetAsync("/api/games/");
        var invalidToken = await anonymous.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "integration-client",
            clientSecret = "bad-secret"
        });

        await client.AuthenticateWithClientCredentialsAsync();

        var authenticatedGames = await client.GetAsync("/api/games/");
        var forbiddenGames = await wrongScope.GetAsync("/api/games/");
        var games = await authenticatedGames.ReadRequiredJsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(anonymousGames.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(invalidToken.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(authenticatedGames.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(forbiddenGames.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(games.GetArrayLength(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task SecurityHeadersCorsAndRateLimitsAreEnforcedAcrossRequests()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        using var apiClient = factory.CreateAuthenticatedClient(SecurityPolicies.UserScope);
        await factory.ResetDatabaseAsync();

        using var allowedCors = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        allowedCors.Headers.Add("Origin", "https://spa.example.test");
        allowedCors.Headers.Add("Access-Control-Request-Method", "GET");

        using var deniedCors = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        deniedCors.Headers.Add("Origin", "https://not-trusted.example.test");
        deniedCors.Headers.Add("Access-Control-Request-Method", "GET");

        var headersResponse = await apiClient.GetAsync("/api/games/");
        var allowedCorsResponse = await client.SendAsync(allowedCors);
        var deniedCorsResponse = await client.SendAsync(deniedCors);

        HttpStatusCode rateLimitStatus = HttpStatusCode.OK;
        for (var i = 0; i < 11; i++)
        {
            rateLimitStatus = (await client.PostAsJsonAsync("/api/auth/token", new
            {
                clientId = "integration-client",
                clientSecret = "bad-secret"
            })).StatusCode;
        }

        Assert.Multiple(() =>
        {
            Assert.That(headersResponse.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(headersResponse.Headers.GetValues("X-Frame-Options").Single(), Is.EqualTo("DENY"));
            Assert.That(headersResponse.Headers.GetValues("Content-Security-Policy").Single(), Does.Contain("frame-ancestors 'none'"));
            Assert.That(allowedCorsResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(
                allowedCorsResponse.Headers.GetValues("Access-Control-Allow-Origin").Single(),
                Is.EqualTo("https://spa.example.test"));
            Assert.That(deniedCorsResponse.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
            Assert.That(rateLimitStatus, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }
}
