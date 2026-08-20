using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class AdminUserEndpointTests
{
    [Test]
    public async Task AdminUserEndpoints_RejectAnonymousAndNonAdminUsers()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(SeedUsers);
        using var anonymousClient = app.CreateClientWithoutAuth();
        using var userClient = app.CreateClientWithScope(SecurityPolicies.UserScope);
        using var adminClient = app.CreateAdminClient("admin-id");

        var anonymous = await anonymousClient.GetAsync("/api/admin/users/");
        var user = await userClient.GetAsync("/api/admin/users/");
        var admin = await adminClient.GetAsync("/api/admin/users/");

        Assert.Multiple(() =>
        {
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(user.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(admin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task AdminUserEndpoints_ListAndUpdateAnotherUsersRole()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(SeedUsers);
        using var adminClient = app.CreateAdminClient("admin-id");

        var listResponse = await adminClient.GetAsync("/api/admin/users/");
        var users = await app.ReadJsonAsync<List<AdminUserSummaryResponse>>(listResponse);

        var promoteResponse = await adminClient.PutAsJsonAsync(
            "/api/admin/users/user-id/role",
            new UpdateUserRoleRequest(SecurityPolicies.AdminRole));
        var promoted = await app.ReadJsonAsync<AdminUserSummaryResponse>(promoteResponse);

        var demoteResponse = await adminClient.PutAsJsonAsync(
            "/api/admin/users/user-id/role",
            new UpdateUserRoleRequest(SecurityPolicies.UserRole));
        var demoted = await app.ReadJsonAsync<AdminUserSummaryResponse>(demoteResponse);

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userRoleId = db.Roles.Single(x => x.Name == SecurityPolicies.UserRole).Id;
        var adminRoleId = db.Roles.Single(x => x.Name == SecurityPolicies.AdminRole).Id;
        var currentRoleIds = db.UserRoles
            .Where(x => x.UserId == "user-id")
            .Select(x => x.RoleId)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(users.Single(x => x.Id == "admin-id").EffectiveRole, Is.EqualTo(SecurityPolicies.AdminRole));
            Assert.That(users.Single(x => x.Id == "admin-id").IsCurrentUser, Is.True);
            Assert.That(users.Single(x => x.Id == "user-id").EffectiveRole, Is.EqualTo(SecurityPolicies.UserRole));
            Assert.That(promoteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(promoted.EffectiveRole, Is.EqualTo(SecurityPolicies.AdminRole));
            Assert.That(promoted.Roles, Contains.Item(SecurityPolicies.UserRole));
            Assert.That(promoted.Roles, Contains.Item(SecurityPolicies.AdminRole));
            Assert.That(demoteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(demoted.EffectiveRole, Is.EqualTo(SecurityPolicies.UserRole));
            Assert.That(demoted.Roles, Contains.Item(SecurityPolicies.UserRole));
            Assert.That(demoted.Roles, Does.Not.Contain(SecurityPolicies.AdminRole));
            Assert.That(currentRoleIds, Contains.Item(userRoleId));
            Assert.That(currentRoleIds, Does.Not.Contain(adminRoleId));
        });
    }

    [Test]
    public async Task AdminUserEndpoints_RejectSelfDemotion()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
            SeedUsers(db, includeSecondAdmin: true));
        using var adminClient = app.CreateAdminClient("admin-id");

        var response = await adminClient.PutAsJsonAsync(
            "/api/admin/users/admin-id/role",
            new UpdateUserRoleRequest(SecurityPolicies.UserRole));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AdminUserEndpoints_RejectLastAdminDemotion()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
            SeedUsers(db, includeSecondAdmin: false));
        using var adminClient = app.CreateAdminClient("external-admin-id");

        var response = await adminClient.PutAsJsonAsync(
            "/api/admin/users/admin-id/role",
            new UpdateUserRoleRequest(SecurityPolicies.UserRole));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static void SeedUsers(ApplicationDbContext db)
    {
        SeedUsers(db, includeSecondAdmin: false);
    }

    private static void SeedUsers(ApplicationDbContext db, bool includeSecondAdmin)
    {
        db.Roles.AddRange(
            new IdentityRole
            {
                Id = "role-user",
                Name = SecurityPolicies.UserRole,
                NormalizedName = SecurityPolicies.UserRole.ToUpperInvariant()
            },
            new IdentityRole
            {
                Id = "role-admin",
                Name = SecurityPolicies.AdminRole,
                NormalizedName = SecurityPolicies.AdminRole.ToUpperInvariant()
            });

        db.Users.AddRange(
            new ApplicationUser
            {
                Id = "admin-id",
                FirstName = "Ada",
                LastName = "Admin",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM"
            },
            new ApplicationUser
            {
                Id = "user-id",
                FirstName = "Una",
                LastName = "User",
                UserName = "user",
                NormalizedUserName = "USER",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM"
            });

        db.UserRoles.AddRange(
            new IdentityUserRole<string>
            {
                UserId = "admin-id",
                RoleId = "role-user"
            },
            new IdentityUserRole<string>
            {
                UserId = "admin-id",
                RoleId = "role-admin"
            },
            new IdentityUserRole<string>
            {
                UserId = "user-id",
                RoleId = "role-user"
            });

        if (includeSecondAdmin)
        {
            db.Users.Add(new ApplicationUser
            {
                Id = "second-admin-id",
                UserName = "second-admin",
                NormalizedUserName = "SECOND-ADMIN",
                Email = "second-admin@example.com",
                NormalizedEmail = "SECOND-ADMIN@EXAMPLE.COM"
            });

            db.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    UserId = "second-admin-id",
                    RoleId = "role-user"
                },
                new IdentityUserRole<string>
                {
                    UserId = "second-admin-id",
                    RoleId = "role-admin"
                });
        }

        db.SaveChanges();
    }
}
