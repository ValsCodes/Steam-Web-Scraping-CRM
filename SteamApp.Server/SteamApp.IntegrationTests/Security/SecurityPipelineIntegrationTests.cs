using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Infrastructure.Identity;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI;
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
            expiresUtc: DateTime.UtcNow.AddSeconds(-1));
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
        var configuration = new Dictionary<string, string?>
        {
            ["Cors__AllowedOrigins__0"] = "http://127.0.0.1:4300",
            ["Cors__AllowedOrigins__1"] = "http://localhost:4300"
        };
        using var factory = new SteamAppFactory(
            environmentName: "Development",
            overrides: configuration);
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        using var allowed = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        allowed.Headers.Add("Origin", "http://127.0.0.1:4300");
        allowed.Headers.Add("Access-Control-Request-Method", "GET");

        using var allowedLocalhost = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        allowedLocalhost.Headers.Add("Origin", "http://localhost:4300");
        allowedLocalhost.Headers.Add("Access-Control-Request-Method", "GET");

        using var denied = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        denied.Headers.Add("Origin", "http://127.0.0.1:4200");
        denied.Headers.Add("Access-Control-Request-Method", "GET");

        using var allowedQuery = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        allowedQuery.Headers.Add("Origin", "http://127.0.0.1:4300");
        allowedQuery.Headers.Add("Access-Control-Request-Method", ApiHttpMethods.Query);

        using var deniedQuery = new HttpRequestMessage(HttpMethod.Options, "/api/games/");
        deniedQuery.Headers.Add("Origin", "http://127.0.0.1:4200");
        deniedQuery.Headers.Add("Access-Control-Request-Method", ApiHttpMethods.Query);

        var allowedResponse = await client.SendAsync(allowed);
        var allowedLocalhostResponse = await client.SendAsync(allowedLocalhost);
        var deniedResponse = await client.SendAsync(denied);
        var allowedQueryResponse = await client.SendAsync(allowedQuery);
        var deniedQueryResponse = await client.SendAsync(deniedQuery);

        Assert.Multiple(() =>
        {
            Assert.That(allowedResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(
                allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single(),
                Is.EqualTo("http://127.0.0.1:4300"));
            Assert.That(
                allowedLocalhostResponse.Headers.GetValues("Access-Control-Allow-Origin").Single(),
                Is.EqualTo("http://localhost:4300"));
            Assert.That(deniedResponse.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
            Assert.That(allowedQueryResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(
                allowedQueryResponse.Headers.GetValues("Access-Control-Allow-Origin").Single(),
                Is.EqualTo("http://127.0.0.1:4300"));
            Assert.That(
                allowedQueryResponse.Headers.GetValues("Access-Control-Allow-Methods").Single(),
                Does.Contain(ApiHttpMethods.Query));
            Assert.That(deniedQueryResponse.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
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
    public async Task SessionRenewal_EnforcesUserScopeLegacyMigrationAndAbsoluteLifetime()
    {
        using var factory = new SteamAppFactory();
        using var anonymous = factory.CreateAnonymousClient();
        using var internalClient = factory.CreateAuthenticatedClient(SecurityPolicies.InternalScope);
        using var validUser = factory.CreateAuthenticatedClient();
        using var legacyUser = factory.CreateAuthenticatedClient(includeAuthenticationTime: false);
        using var expiredSession = factory.CreateAuthenticatedClient(
            authenticationTimeUtc: DateTime.UtcNow.AddHours(-4).AddMinutes(-1));
        await factory.ResetDatabaseAsync();

        var anonymousResponse = await anonymous.PostAsync("/api/auth/renew", null);
        var internalResponse = await internalClient.PostAsync("/api/auth/renew", null);
        var validResponse = await validUser.PostAsync("/api/auth/renew", null);
        var legacyResponse = await legacyUser.PostAsync("/api/auth/renew", null);
        var expiredResponse = await expiredSession.PostAsync("/api/auth/renew", null);

        Assert.Multiple(() =>
        {
            Assert.That(anonymousResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(internalResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(validResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(legacyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(expiredResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task LoginLocksOutUserAfterConfiguredFailedAttempts()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var failedAttemptBodies = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                emailOrUserName = IntegrationSeed.UserName,
                password = "WrongPassword1"
            });

            failedAttemptBodies.Add(await response.Content.ReadAsStringAsync());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        var lockoutResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserName,
            password = "WrongPassword1"
        });
        var lockoutBody = await lockoutResponse.Content.ReadAsStringAsync();

        var correctPasswordDuringLockout = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserName,
            password = IntegrationSeed.UserPassword
        });
        var correctPasswordDuringLockoutBody =
            await correctPasswordDuringLockout.Content.ReadAsStringAsync();

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync(IntegrationSeed.UserName);

        Assert.That(user, Is.Not.Null);

        var lockoutEnd = await userManager.GetLockoutEndDateAsync(user!);

        Assert.Multiple(() =>
        {
            Assert.That(failedAttemptBodies, Is.All.EqualTo("Invalid user credentials."));
            Assert.That(lockoutResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(lockoutBody, Is.EqualTo("User account is locked out."));
            Assert.That(correctPasswordDuringLockout.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(correctPasswordDuringLockoutBody, Is.EqualTo("User account is locked out."));
            Assert.That(lockoutEnd, Is.Not.Null);
            Assert.That(lockoutEnd!.Value, Is.GreaterThan(DateTimeOffset.UtcNow));
        });
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

    [Test]
    public async Task ProfileEndpointsUseIdentityStore()
    {
        using var factory = new SteamAppFactory();
        using var authenticated = factory.CreateAuthenticatedClient();
        using var anonymous = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var profile = await authenticated.GetAsync("/api/auth/profile");
        var profileJson = await profile.ReadJsonElementAsync();

        var update = await authenticated.PutAsJsonAsync("/api/auth/profile", new
        {
            firstName = "Updated",
            lastName = "User",
            userName = "updated-user",
            email = "updated-user@example.com",
            phone = "+3595550100"
        });
        var updatedJson = await update.ReadJsonElementAsync();

        var password = await authenticated.PutAsJsonAsync("/api/auth/profile/password", new
        {
            currentPassword = IntegrationSeed.UserPassword,
            newPassword = "Password2"
        });

        var oldPasswordLogin = await anonymous.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "updated-user@example.com",
            password = IntegrationSeed.UserPassword
        });
        var newPasswordLogin = await anonymous.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "updated-user",
            password = "Password2"
        });

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/auth/profile")
        {
            Content = JsonContent.Create(new { password = "Password2" })
        };
        var delete = await authenticated.SendAsync(deleteRequest);
        var deletedLogin = await anonymous.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "updated-user",
            password = "Password2"
        });

        Assert.Multiple(() =>
        {
            Assert.That(profile.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(profileJson.GetProperty("email").GetString(), Is.EqualTo(IntegrationSeed.UserEmail));
            Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedJson.GetProperty("displayName").GetString(), Is.EqualTo("Updated User"));
            Assert.That(updatedJson.GetProperty("phone").GetString(), Is.EqualTo("+3595550100"));
            Assert.That(password.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(oldPasswordLogin.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(newPasswordLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deletedLogin.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }
}
