using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.OperationResults;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Services;

public sealed class ExternalAccountRegistration(IServiceScopeFactory scopes)
{
    internal static readonly Error AccountConflict = new(
        "external_account_conflict",
        "Sign in to your existing SteamApp account and connect the provider from Profile.",
        ErrorType.Conflict);

    public async Task<Result<string>> RegisterAsync(
        UserLoginInfo login, string email, CancellationToken cancellationToken)
    {
        await using var strategyScope = scopes.CreateAsyncScope();
        var strategyContext = strategyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var strategy = strategyContext.Database.CreateExecutionStrategy();
        var userId = Guid.NewGuid().ToString();

        try
        {
            return await strategy.ExecuteAsync(async ct =>
            {
                // A retry must use a fresh tracker and the same candidate user ID.
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                var existing = await users.FindByLoginAsync(login.LoginProvider, login.ProviderKey);
                if (existing != null)
                {
                    return existing.Id == userId
                        ? Result<string>.Success(existing.Id)
                        : Result<string>.Failure(AccountConflict);
                }
                if (await users.FindByEmailAsync(email) != null)
                {
                    return Result<string>.Failure(AccountConflict);
                }

                var user = new ApplicationUser
                {
                    Id = userId,
                    UserName = userId,
                    Email = email,
                    EmailConfirmed = true
                };
                var created = await users.CreateAsync(user);
                if (!created.Succeeded)
                {
                    if (created.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.DuplicateEmail)))
                    {
                        return Result<string>.Failure(AccountConflict);
                    }
                    return Result<string>.Failure(new Error(
                        "external_registration_invalid", "The email could not be used to register.", ErrorType.Validation));
                }

                var linked = await users.AddLoginAsync(user, login);
                if (!linked.Succeeded)
                {
                    return Result<string>.Failure(AccountConflict);
                }

                var role = await users.AddToRoleAsync(user, SecurityPolicies.UserRole);
                if (!role.Succeeded)
                {
                    return Result<string>.Failure(new Error(
                        "external_registration_unavailable",
                        "Account registration is temporarily unavailable.",
                        ErrorType.Unavailable));
                }

                await transaction.CommitAsync(ct);
                return Result<string>.Success(user.Id);
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConflict(exception))
        {
            return Result<string>.Failure(AccountConflict);
        }
    }

    internal static bool IsUniqueConflict(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
