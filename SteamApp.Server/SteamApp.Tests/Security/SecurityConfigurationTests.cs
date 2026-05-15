using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Security;

[TestFixture]
public sealed class SecurityConfigurationTests
{
    private const string ValidJwtKey = "12345678901234567890123456789012";

    [Test]
    public void ValidateJwtSettings_RejectsShortSigningKey()
    {
        Assert.That(
            () => InvokePrivate(
                "ValidateJwtSettings",
                new JwtSettings
                {
                    Key = "too-short",
                    Issuer = "issuer",
                    Audience = "audience",
                    DurationMinutes = 60
                }),
            Throws.InvalidOperationException.With.Message.Contains("at least 32 bytes"));
    }

    [TestCase(0)]
    [TestCase(121)]
    public void ValidateJwtSettings_RejectsDurationOutsideAllowedRange(int duration)
    {
        Assert.That(
            () => InvokePrivate(
                "ValidateJwtSettings",
                new JwtSettings
                {
                    Key = ValidJwtKey,
                    Issuer = "issuer",
                    Audience = "audience",
                    DurationMinutes = duration
                }),
            Throws.InvalidOperationException.With.Message.Contains("between 1 and 120"));
    }

    [Test]
    public void ValidateClientDefinitions_RejectsDuplicateClientIds()
    {
        var clients = new[]
        {
            ValidClient("client"),
            ValidClient("client")
        };

        Assert.That(
            () => InvokePrivate("ValidateClientDefinitions", clients, Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("Duplicate client definition"));
    }

    [Test]
    public void ValidateClientDefinitions_RequiresClientId()
    {
        var clients = new[]
        {
            ValidClient("")
        };

        Assert.That(
            () => InvokePrivate("ValidateClientDefinitions", clients, Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("must include ClientId"));
    }

    [Test]
    public void ValidateClientDefinitions_RejectsUnsupportedScope()
    {
        var client = ValidClient("client");
        client.AllowedScope = "admin";

        Assert.That(
            () => InvokePrivate(
                "ValidateClientDefinitions",
                new[] { client },
                Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("unsupported AllowedScope"));
    }

    [Test]
    public void ValidateClientDefinitions_RequiresASecret()
    {
        var client = new ClientDefinition
        {
            ClientId = "client",
            AllowedScope = SecurityPolicies.UserScope
        };

        Assert.That(
            () => InvokePrivate(
                "ValidateClientDefinitions",
                new[] { client },
                Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("must define ClientSecretHash"));
    }

    [Test]
    public void ValidateClientDefinitions_RejectsInvalidHash()
    {
        var client = ValidClient("client");
        client.ClientSecretHash = "not-a-sha256-hash";

        Assert.That(
            () => InvokePrivate(
                "ValidateClientDefinitions",
                new[] { client },
                Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("SHA-256 hex hash"));
    }

    [Test]
    public void ValidateClientDefinitions_RejectsPlainTextSecretOutsideDevelopment()
    {
        var client = new ClientDefinition
        {
            ClientId = "client",
            ClientSecret = "secret",
            AllowedScope = SecurityPolicies.UserScope
        };

        Assert.That(
            () => InvokePrivate(
                "ValidateClientDefinitions",
                new[] { client },
                Environment(Environments.Production)),
            Throws.InvalidOperationException.With.Message.Contains("must use ClientSecretHash"));
    }

    [Test]
    public void ValidateClientDefinitions_AllowsPlainTextSecretInDevelopment()
    {
        var client = new ClientDefinition
        {
            ClientId = "client",
            ClientSecret = "secret",
            AllowedScope = SecurityPolicies.UserScope
        };

        Assert.DoesNotThrow(() =>
            InvokePrivate(
                "ValidateClientDefinitions",
                new[] { client },
                Environment(Environments.Development)));
    }

    [TestCase(null)]
    [TestCase("*")]
    public void ValidateHostFilteringConfiguration_RequiresExplicitHostsOutsideDevelopment(string? allowedHosts)
    {
        var config = Config(("AllowedHosts", allowedHosts));

        Assert.That(
            () => InvokePrivate(
                "ValidateHostFilteringConfiguration",
                config,
                Environment(Environments.Production)),
            Throws.InvalidOperationException.With.Message.Contains("AllowedHosts"));
    }

    [Test]
    public void ValidateHostFilteringConfiguration_AllowsExplicitHostsOutsideDevelopment()
    {
        var config = Config(("AllowedHosts", "api.example.com;admin.example.com"));

        Assert.DoesNotThrow(() =>
            InvokePrivate(
                "ValidateHostFilteringConfiguration",
                config,
                Environment(Environments.Production)));
    }

    [Test]
    public void ValidateCorsOrigins_AllowsEmptyOriginsInDevelopment()
    {
        Assert.DoesNotThrow(() =>
            InvokePrivate(
                "ValidateCorsOrigins",
                Array.Empty<string>(),
                Environment(Environments.Development)));
    }

    [Test]
    public void ValidateCorsOrigins_RejectsInvalidOrigin()
    {
        Assert.That(
            () => InvokePrivate(
                "ValidateCorsOrigins",
                new[] { "not a url" },
                Environment(Environments.Development)),
            Throws.InvalidOperationException.With.Message.Contains("invalid origin"));
    }

    [Test]
    public void ValidateCorsOrigins_RequiresHttpsOutsideDevelopment()
    {
        Assert.That(
            () => InvokePrivate(
                "ValidateCorsOrigins",
                new[] { "http://app.example.com" },
                Environment(Environments.Production)),
            Throws.InvalidOperationException.With.Message.Contains("must use HTTPS"));
    }

    [Test]
    public void BuildApiAuthorizationPolicy_RequiresJwtBearerAndUserOrInternalScope()
    {
        var policy = InvokePrivate<AuthorizationPolicy>("BuildApiAuthorizationPolicy");
        var scopeRequirement = policy.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Single(x => x.ClaimType == "scope");

        Assert.Multiple(() =>
        {
            Assert.That(policy.AuthenticationSchemes, Contains.Item(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(
                scopeRequirement.AllowedValues,
                Is.EquivalentTo(new[] { SecurityPolicies.UserScope, SecurityPolicies.InternalScope }));
        });
    }

    [Test]
    public void ShouldEnsureIdentitySchema_DefaultsToDevelopmentOnlyAndHonorsOverride()
    {
        var devDefault = InvokePrivate<bool>(
            "ShouldEnsureIdentitySchema",
            Config(),
            Environment(Environments.Development));
        var prodDefault = InvokePrivate<bool>(
            "ShouldEnsureIdentitySchema",
            Config(),
            Environment(Environments.Production));
        var overrideFalse = InvokePrivate<bool>(
            "ShouldEnsureIdentitySchema",
            Config(("Database:EnsureIdentitySchemaOnStartup", "false")),
            Environment(Environments.Development));

        Assert.Multiple(() =>
        {
            Assert.That(devDefault, Is.True);
            Assert.That(prodDefault, Is.False);
            Assert.That(overrideFalse, Is.False);
        });
    }

    [Test]
    public async Task SecurityHeadersMiddleware_AddsExpectedHeaders()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseSecurityHeaders();
        app.MapGet("/", () => Results.Ok());

        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.Multiple(() =>
        {
            Assert.That(response.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(response.Headers.GetValues("X-Frame-Options").Single(), Is.EqualTo("DENY"));
            Assert.That(response.Headers.GetValues("Referrer-Policy").Single(), Is.EqualTo("no-referrer"));
            Assert.That(response.Headers.GetValues("Permissions-Policy").Single(), Does.Contain("geolocation=()"));
            Assert.That(response.Headers.GetValues("Content-Security-Policy").Single(), Does.Contain("default-src 'self'"));
        });

        await app.DisposeAsync();
    }

    [Test]
    public async Task MinimalApiRequests_RequireAuthenticatedUserWithAllowedScope()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        using var noAuthClient = app.CreateClientWithoutAuth();
        using var invalidScopeClient = app.CreateClientWithScope("other");

        var noAuth = await noAuthClient.GetAsync("/api/games/");
        var invalidScope = await invalidScopeClient.GetAsync("/api/games/");
        var validScope = await app.Client.GetAsync("/api/games/");

        Assert.Multiple(() =>
        {
            Assert.That(noAuth.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
            Assert.That(invalidScope.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
            Assert.That(validScope.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task MinimalApiEndpoints_AllRequireApiUserPolicyAndApiRateLimit()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);
        var endpoints = ((IEndpointRouteBuilder)app.App).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(x => x.RoutePattern.RawText?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();

        Assert.That(endpoints, Is.Not.Empty);

        Assert.Multiple(() =>
        {
            foreach (var endpoint in endpoints)
            {
                var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
                Assert.That(
                    authorizeData.Any(x => x.Policy == SecurityPolicies.ApiUser),
                    Is.True,
                    $"{endpoint.RoutePattern.RawText} should require {SecurityPolicies.ApiUser}.");

                Assert.That(
                    endpoint.Metadata.Any(HasApiRateLimitPolicy),
                    Is.True,
                    $"{endpoint.RoutePattern.RawText} should require {SecurityPolicies.ApiRateLimit}.");
            }
        });
    }

    private static bool HasApiRateLimitPolicy(object metadata)
    {
        var policyName = metadata.GetType().GetProperty("PolicyName")?.GetValue(metadata)?.ToString();
        return policyName == SecurityPolicies.ApiRateLimit;
    }

    private static ClientDefinition ValidClient(string clientId)
    {
        return new ClientDefinition
        {
            ClientId = clientId,
            ClientSecretHash = new string('A', 64),
            AllowedScope = SecurityPolicies.UserScope
        };
    }

    private static IConfiguration Config(params (string Key, string? Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => x.Value))
            .Build();
    }

    private static IHostEnvironment Environment(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(name);
        return environment.Object;
    }

    private static void InvokePrivate(string methodName, params object?[] args)
    {
        InvokePrivate<object?>(methodName, args);
    }

    private static T InvokePrivate<T>(string methodName, params object?[] args)
    {
        var method = typeof(Program).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(method, Is.Not.Null, $"Expected Program.{methodName} to exist.");

        try
        {
            return (T)method!.Invoke(null, args)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}
