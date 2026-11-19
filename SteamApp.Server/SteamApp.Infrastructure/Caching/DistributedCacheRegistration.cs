using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SteamApp.Infrastructure.Caching;

public static class DistributedCacheRegistration
{
    public static void AddSteamAppDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Redis:Enabled"))
        {
            services.AddDistributedMemoryCache();
            return;
        }

        var connectionString =
            configuration.GetConnectionString("Redis")
            ?? configuration.GetConnectionString("CacheConnection")
            ?? configuration["Redis:ConnectionString"]
            ?? configuration["CacheConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Redis:ConnectionString must be configured when Redis:Enabled is true.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "SteamApp:";
        });
    }
}
