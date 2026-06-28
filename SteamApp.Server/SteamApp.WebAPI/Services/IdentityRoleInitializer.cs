using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Services;

public sealed class IdentityRoleInitializer(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<IdentityRoleInitializer> logger)
{
    private static readonly string[] RequiredRoles =
    [
        SecurityPolicies.UserRole,
        SecurityPolicies.AdminRole
    ];

    public async Task EnsureRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var role in RequiredRoles)
            {
                await EnsureRoleExistsAsync(role);
            }

            var adminEmails = GetConfiguredAdminEmails();
            var users = await userManager.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                await EnsureUserRoleAsync(user, SecurityPolicies.UserRole);

                if (IsConfiguredAdmin(user, adminEmails))
                {
                    await EnsureUserRoleAsync(user, SecurityPolicies.AdminRole);
                }
            }
        }
        catch (Exception ex) when (IsMissingIdentitySchemaException(ex))
        {
            logger.LogWarning(
                ex,
                "Identity role initialization skipped because the Identity schema is not available.");
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        ThrowIfFailed(result, $"create role '{roleName}'");
    }

    private async Task EnsureUserRoleAsync(ApplicationUser user, string roleName)
    {
        if (await userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        ThrowIfFailed(result, $"assign role '{roleName}' to user '{user.Id}'");
    }

    private HashSet<string> GetConfiguredAdminEmails()
    {
        var emails = configuration
            .GetSection("Authentication:AdminEmails")
            .Get<string[]>() ?? [];

        return emails
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsConfiguredAdmin(
        ApplicationUser user,
        HashSet<string> adminEmails)
    {
        return !string.IsNullOrWhiteSpace(user.Email) &&
               adminEmails.Contains(user.Email);
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(x => x.Description));
        throw new InvalidOperationException($"Unable to {action}: {errors}");
    }

    private static bool IsMissingIdentitySchemaException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is not DbException)
            {
                continue;
            }

            var message = current.Message;
            if (message.Contains("AspNetRoles", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("AspNetUsers", StringComparison.OrdinalIgnoreCase))
            {
                return message.Contains("no such table", StringComparison.OrdinalIgnoreCase) ||
                       message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                       message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }
}
