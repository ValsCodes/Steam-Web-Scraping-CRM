using System.Net.Http.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SteamApp.Application.Mapper;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.TestSupport;

public sealed class MinimalApiTestApp : IAsyncDisposable
{
    private MinimalApiTestApp(WebApplication app, HttpClient client, string databaseName)
    {
        App = app;
        Client = client;
        DatabaseName = databaseName;
    }

    public WebApplication App { get; }
    public HttpClient Client { get; }
    public string DatabaseName { get; }

    public static async Task<MinimalApiTestApp> CreateAsync(Action<ApplicationDbContext>? seed = null)
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddRouting();
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseInMemoryDatabase(databaseName));
        builder.Services.AddAutoMapper(_ => { }, typeof(BaseProfile));
        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services
            .AddAuthentication(FakeAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                FakeAuthenticationHandler.SchemeName,
                _ => { });

        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy(SecurityPolicies.ApiUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(
                    "scope",
                    SecurityPolicies.UserScope,
                    SecurityPolicies.InternalScope);
            });

            opts.AddPolicy(SecurityPolicies.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", SecurityPolicies.UserScope);
                policy.RequireRole(SecurityPolicies.AdminRole);
            });
        });

        builder.Services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.AddPolicy(SecurityPolicies.ApiRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.Identity?.Name ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            seed?.Invoke(db);
        }

        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapGameEndpoints();
        app.MapGameUrlEndpoints();
        app.MapScrapingModeEndpoints();
        app.MapProductEndpoints();
        app.MapPixelEndpoints();
        app.MapWatchListEndpoints();
        app.MapWishListEndpoints();
        app.MapFeedbackRequestEndpoints();
        app.MapGameUrlProductsEndpoints();
        app.MapTagsEndpoints();
        app.MapProductTagsEndpoints();
        app.MapGameUrlPixelsEndpoints();
        app.MapAdminUserEndpoints();

        await app.StartAsync();

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add(
            FakeAuthenticationHandler.ScopeHeader,
            SecurityPolicies.UserScope);
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        return new MinimalApiTestApp(app, client, databaseName);
    }

    public HttpClient CreateClientWithoutAuth()
    {
        var client = App.GetTestClient();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    public HttpClient CreateClientWithScope(string scope)
    {
        var client = CreateClientWithoutAuth();
        client.DefaultRequestHeaders.Add(FakeAuthenticationHandler.ScopeHeader, scope);
        return client;
    }

    public HttpClient CreateAdminClient(string userId = TestDb.TestUserId)
    {
        var client = CreateClientWithScope(SecurityPolicies.UserScope);
        client.DefaultRequestHeaders.Add(FakeAuthenticationHandler.RoleHeader, SecurityPolicies.AdminRole);
        client.DefaultRequestHeaders.Add(FakeAuthenticationHandler.UserIdHeader, userId);
        return client;
    }

    public async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>();
        Assert.That(result, Is.Not.Null);
        return result!;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
    }
}
