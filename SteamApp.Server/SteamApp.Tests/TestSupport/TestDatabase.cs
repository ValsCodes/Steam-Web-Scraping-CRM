using SteamApp.Infrastructure.Persistence;

namespace SteamApp.Tests.TestSupport;

public sealed class TestDatabase(
    ApplicationDbContext context,
    TestDbContextFactory factory)
    : IDisposable
{
    public ApplicationDbContext Context { get; } = context;
    public TestDbContextFactory Factory { get; } = factory;

    public void Dispose()
    {
        Context.Dispose();
    }
}
