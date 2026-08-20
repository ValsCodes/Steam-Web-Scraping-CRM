using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class AdminUserEndpoints
{
    public static WebApplication MapAdminUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/admin/users")
                       .WithTags("AdminUsers")
                       .RequireAuthorization(SecurityPolicies.AdminOnly)
                       .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        group.MapGet("/", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            CancellationToken ct) =>
        {
            var currentUserId = httpContext.User.GetUserId();
            if (currentUserId is null) { return Results.Unauthorized(); }

            var users = await userManager.Users
                .AsNoTracking()
                .OrderBy(x => x.Email)
                .ThenBy(x => x.UserName)
                .ToListAsync(ct);

            var summaries = new List<AdminUserSummaryResponse>(users.Count);
            foreach (var user in users)
            {
                summaries.Add(await CreateSummaryAsync(userManager, user, currentUserId));
            }

            return Results.Ok(summaries);
        })
        .WithName("GetAdminUsers")
        .Produces<List<AdminUserSummaryResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{id}/role", async (
            string id,
            UpdateUserRoleRequest input,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            CancellationToken ct) =>
        {
            var currentUserId = httpContext.User.GetUserId();
            if (currentUserId is null) { return Results.Unauthorized(); }

            var requestedRole = NormalizeRequestedRole(input.Role);
            if (requestedRole is null)
            {
                return Results.BadRequest(new { message = "Role must be Admin or User." });
            }

            var user = await userManager.FindByIdAsync(id);
            if (user is null) { return Results.NotFound(); }

            var isAdmin = await userManager.IsInRoleAsync(user, SecurityPolicies.AdminRole);
            var demotingToUser = requestedRole == SecurityPolicies.UserRole && isAdmin;

            if (demotingToUser &&
                string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
            {
                return Results.BadRequest(new { message = "Admins cannot demote themselves." });
            }

            if (demotingToUser && await CountAdminsAsync(userManager, ct) <= 1)
            {
                return Results.BadRequest(new { message = "The last Admin cannot be demoted." });
            }

            var userRoleResult = await EnsureRoleAsync(
                userManager,
                user,
                SecurityPolicies.UserRole);
            if (userRoleResult is not null) { return userRoleResult; }

            if (requestedRole == SecurityPolicies.AdminRole)
            {
                var adminRoleResult = await EnsureRoleAsync(
                    userManager,
                    user,
                    SecurityPolicies.AdminRole);
                if (adminRoleResult is not null) { return adminRoleResult; }
            }
            else if (isAdmin)
            {
                var result = await userManager.RemoveFromRoleAsync(user, SecurityPolicies.AdminRole);
                if (!result.Succeeded)
                {
                    return CreateValidationProblem(result);
                }
            }

            return Results.Ok(await CreateSummaryAsync(userManager, user, currentUserId));
        })
        .WithName("UpdateAdminUserRole")
        .Accepts<UpdateUserRoleRequest>("application/json")
        .Produces<AdminUserSummaryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult?> EnsureRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string role)
    {
        if (await userManager.IsInRoleAsync(user, role))
        {
            return null;
        }

        var result = await userManager.AddToRoleAsync(user, role);
        return result.Succeeded ? null : CreateValidationProblem(result);
    }

    private static async Task<int> CountAdminsAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken ct)
    {
        var admins = await userManager.GetUsersInRoleAsync(SecurityPolicies.AdminRole);
        return admins.Count(user => !ct.IsCancellationRequested && user.Id is not null);
    }

    private static string? NormalizeRequestedRole(string? role)
    {
        if (string.Equals(role, SecurityPolicies.AdminRole, StringComparison.OrdinalIgnoreCase))
        {
            return SecurityPolicies.AdminRole;
        }

        if (string.Equals(role, SecurityPolicies.UserRole, StringComparison.OrdinalIgnoreCase))
        {
            return SecurityPolicies.UserRole;
        }

        return null;
    }

    private static async Task<AdminUserSummaryResponse> CreateSummaryAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string currentUserId)
    {
        var roles = OrderKnownRoles(await userManager.GetRolesAsync(user));
        var effectiveRole = roles.Contains(SecurityPolicies.AdminRole, StringComparer.OrdinalIgnoreCase)
            ? SecurityPolicies.AdminRole
            : SecurityPolicies.UserRole;

        return new AdminUserSummaryResponse(
            Id: user.Id,
            DisplayName: CreateDisplayName(user) ?? user.UserName ?? user.Email ?? user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            UserName: user.UserName,
            Email: user.Email,
            Phone: user.PhoneNumber,
            Roles: roles,
            EffectiveRole: effectiveRole,
            IsCurrentUser: string.Equals(user.Id, currentUserId, StringComparison.Ordinal));
    }

    private static string[] OrderKnownRoles(IEnumerable<string> roles)
    {
        return roles
            .Select(NormalizeKnownRole)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role == SecurityPolicies.UserRole ? 0 : role == SecurityPolicies.AdminRole ? 1 : 2)
            .ThenBy(role => role, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeKnownRole(string role)
    {
        if (string.Equals(role, SecurityPolicies.AdminRole, StringComparison.OrdinalIgnoreCase))
        {
            return SecurityPolicies.AdminRole;
        }

        if (string.Equals(role, SecurityPolicies.UserRole, StringComparison.OrdinalIgnoreCase))
        {
            return SecurityPolicies.UserRole;
        }

        return role;
    }

    private static string? CreateDisplayName(ApplicationUser user)
    {
        var fullName = string.Join(
            ' ',
            new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim()));

        return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
    }

    private static IResult CreateValidationProblem(IdentityResult result)
    {
        return Results.ValidationProblem(
            result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray()));
    }
}

public sealed record AdminUserSummaryResponse(
    string Id,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string? UserName,
    string? Email,
    string? Phone,
    string[] Roles,
    string EffectiveRole,
    bool IsCurrentUser);

public sealed record UpdateUserRoleRequest(string? Role);
