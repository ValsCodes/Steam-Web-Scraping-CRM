using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Security;

namespace SteamApp.IntegrationTests.Security;

[TestFixture]
public sealed class SecurityPipelineIntegrationTests
{
    [Test]
    public async Task ProtectedEndpointsRejectMissingWrongAndExpiredTokens()
    {
        using var factory = new SteamAppFactory();
        using var anonymous = factory.CreateAnonymousClient();
        using var wrongScope = factory.CreateAuthenticatedClient("other");
        using var expired = factory.CreateAuthenticatedClient(
            expiresUtc: DateTime.UtcNow.AddMinutes(-5));
        using var valid = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var anonymousResponse = await anonymous.GetAsync("/api/games/");
        var wrongScopeResponse = await wrongScope.GetAsync("/api/games/");
        var expiredResponse = await expired.GetAsync("/api/games/");
        var validResponse = await valid.GetAsync("/api/games/");

        Assert.Multiple(() =>
        {
            Assert.That(anonymousResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(wrongScopeResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(expiredResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(validResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task SecurityHeadersAreAddedToApiResponses()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/api/games/");

        Assert.Multiple(() =>
        {
            Assert.That(response.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(response.Headers.GetValues("X-Frame-Options").Single(), Is.EqualTo("DENY"));
            Assert.That(response.Headers.GetValues("Referrer-Policy").Single(), Is.EqualTo("no-referrer"));
            Assert.That(response.Headers.GetValues("Permissions-Policy").Single(), Does.Contain("camera=()"));
            Assert.That(response.Headers.GetValues("Content-Security-Policy").Single(), Does.Contain("frame-ancestors 'none'"));
        });
    }

    [Test]
    public async Task HttpsRedirectionRedirectsHttpRequests()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });

        var response = await client.GetAsync("/api/games/");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RedirectKeepVerb));
    }

    [Test]
    public async Task CorsAllowsConfiguredOriginAndRejectsUnconfiguredOrigin()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        using var allowed = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        allowed.Headers.Add("Origin", "https://spa.example.test");
        allowed.Headers.Add("Access-Control-Request-Method", "GET");

        using var denied = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        denied.Headers.Add("Origin", "https://evil.example.test");
        denied.Headers.Add("Access-Control-Request-Method", "GET");

        var allowedResponse = await client.SendAsync(allowed);
        var deniedResponse = await client.SendAsync(denied);

        Assert.Multiple(() =>
        {
            Assert.That(allowedResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(
                allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single(),
                Is.EqualTo("https://spa.example.test"));
            Assert.That(deniedResponse.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
        });
    }

    [Test]
    public async Task SwaggerIsAvailableInDevelopmentOnly()
    {
        using var devFactory = new SteamAppFactory(environmentName: "Development");
        using var prodFactory = new SteamAppFactory(environmentName: "Production");
        using var devClient = devFactory.CreateAnonymousClient();
        using var prodClient = prodFactory.CreateAuthenticatedClient();

        var dev = await devClient.GetAsync("/swagger/v1/swagger.json");
        var prod = await prodClient.GetAsync("/swagger/v1/swagger.json");

        Assert.Multiple(() =>
        {
            Assert.That(dev.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(prod.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task ProductionResponsesIncludeHsts()
    {
        using var factory = new SteamAppFactory(environmentName: "Production");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://app.example.test")
        });
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/api/games/");

        Assert.That(response.Headers.Contains("Strict-Transport-Security"), Is.True);
    }

    [Test]
    public async Task AuthEndpointIsRateLimited()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var responses = new List<HttpStatusCode>();
        for (var i = 0; i < 11; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/token", new
            {
                clientId = "integration-client",
                clientSecret = "wrong"
            });
            responses.Add(response.StatusCode);
        }

        Assert.That(responses.Last(), Is.EqualTo(HttpStatusCode.TooManyRequests));
    }

    [Test]
    public async Task ApiAndExpensiveApiEndpointsAreRateLimited()
    {
        using var factory = new SteamAppFactory();
        using var apiClient = factory.CreateAuthenticatedClient();
        using var expensiveClient = factory.CreateAuthenticatedClient(SecurityPolicies.UserScope);
        await factory.ResetDatabaseAsync();

        HttpStatusCode apiStatus = HttpStatusCode.OK;
        for (var i = 0; i < 121; i++)
        {
            apiStatus = (await apiClient.GetAsync("/api/games/")).StatusCode;
        }

        HttpStatusCode expensiveStatus = HttpStatusCode.OK;
        for (var i = 0; i < 21; i++)
        {
            expensiveStatus = (await expensiveClient.GetAsync("/steam/scrape-page/gameUrl/1/page/1")).StatusCode;
        }

        Assert.Multiple(() =>
        {
            Assert.That(apiStatus, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(expensiveStatus, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }

    [Test]
    public async Task TokenEndpointReturnsJwtForConfiguredClient()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var response = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "integration-client",
            clientSecret = "integration-secret"
        });
        var json = await response.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.GetProperty("token").GetString(), Is.Not.Empty);
            Assert.That(json.GetProperty("tokenType").GetString(), Is.EqualTo("Bearer"));
        });
    }

    [Test]
    public async Task TokenEndpointRejectsBadClientCredentials()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "integration-client",
            clientSecret = "bad"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task LoginAndRegisterUseIdentityStore()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserEmail,
            password = IntegrationSeed.UserPassword
        });
        var userNameLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserName,
            password = IntegrationSeed.UserPassword
        });
        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserName,
            password = "bad"
        });
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new-user@example.com",
            userName = "new-user",
            password = "Password1"
        });

        Assert.Multiple(() =>
        {
            Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(userNameLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(wrongPassword.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(register.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }
}
