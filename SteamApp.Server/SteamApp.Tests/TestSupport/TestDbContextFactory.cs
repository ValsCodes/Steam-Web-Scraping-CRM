using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Context;

namespace SteamApp.Tests.TestSupport;

public sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(options);
    }
}
