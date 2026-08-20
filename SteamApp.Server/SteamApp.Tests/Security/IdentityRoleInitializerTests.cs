using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Security;

[TestFixture]
public sealed class IdentityRoleInitializerTests
{
    [Test]
    public async Task EnsureRolesAsync_CreatesRequiredRoles()
    {
        await using var provider = CreateProvider(Config());
        await using var scope = provider.CreateAsyncScope();

        var initializer = scope.ServiceProvider.GetRequiredService<IdentityRoleInitializer>();
        await initializer.EnsureRolesAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userRoleExists = await roleManager.RoleExistsAsync(SecurityPolicies.UserRole);
        var adminRoleExists = await roleManager.RoleExistsAsync(SecurityPolicies.AdminRole);

        Assert.Multiple(() =>
        {
            Assert.That(userRoleExists, Is.True);
            Assert.That(adminRoleExists, Is.True);
        });
    }

    [Test]
    public async Task EnsureRolesAsync_AssignsUserAndConfiguredAdminRoles()
    {
        await using var provider = CreateProvider(Config(
            ("Authentication:AdminEmails:0", "owner@example.com")));
        await using var scope = provider.CreateAsyncScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var owner = new ApplicationUser
        {
            UserName = "owner",
            Email = "OWNER@example.com"
        };
        var member = new ApplicationUser
        {
            UserName = "member",
            Email = "member@example.com"
        };
        await userManager.CreateAsync(owner);
        await userManager.CreateAsync(member);

        var initializer = scope.ServiceProvider.GetRequiredService<IdentityRoleInitializer>();
        await initializer.EnsureRolesAsync();

        var ownerIsUser = await userManager.IsInRoleAsync(owner, SecurityPolicies.UserRole);
        var ownerIsAdmin = await userManager.IsInRoleAsync(owner, SecurityPolicies.AdminRole);
        var memberIsUser = await userManager.IsInRoleAsync(member, SecurityPolicies.UserRole);
        var memberIsAdmin = await userManager.IsInRoleAsync(member, SecurityPolicies.AdminRole);

        Assert.Multiple(() =>
        {
            Assert.That(ownerIsUser, Is.True);
            Assert.That(ownerIsAdmin, Is.True);
            Assert.That(memberIsUser, Is.True);
            Assert.That(memberIsAdmin, Is.False);
        });
    }

    private static ServiceProvider CreateProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<IdentityRoleInitializer>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        return provider;
    }

    private static IConfiguration Config(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
    }
}
