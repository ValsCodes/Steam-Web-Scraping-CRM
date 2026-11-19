using System.Data;
using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Persistence;

namespace SteamApp.WebAPI.Security;

public sealed class AccountMutation(ApplicationDbContext db, IConfiguration configuration)
{
    private static readonly SemaphoreSlim NonRelationalGate = new(1, 1);

    public bool IsConfiguredAdmin(string userId) =>
        configuration.GetSection("Authentication:AdminUserIds").Get<string[]>()?.Any(
            id => string.Equals(id?.Trim(), userId, StringComparison.Ordinal)) == true;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            await NonRelationalGate.WaitAsync(cancellationToken);
            try
            {
                return await operation();
            }
            finally
            {
                NonRelationalGate.Release();
            }
        }

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async ct =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var result = await operation();
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);
    }
}
