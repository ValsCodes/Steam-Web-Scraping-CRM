using Microsoft.EntityFrameworkCore;
using SteamApp.Application.Abstractions.Persistence;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Application.Features.ManualChecks;
using SteamApp.Application.Features.Scraping;
using SteamApp.Application.Features.Wishlist;
using SteamApp.Application.Mappings;
using SteamApp.Infrastructure.Caching;
using SteamApp.Infrastructure.ExternalServices.ManualChecks;
using SteamApp.Infrastructure.ExternalServices;
using SteamApp.Infrastructure.Identity;
using SteamApp.Infrastructure.Messaging.RabbitMq.DependencyInjection;
using SteamApp.Infrastructure.Persistence.Repositories;
using SteamApp.Infrastructure.Persistence;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Workers.Base;
using SteamApp.WebAPI.Workers;

namespace SteamApp.WebAPI.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    internal static void AddSteamAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<ApplicationDbContext>(opts =>
        {
            opts.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        });

        builder.Services.AddAutoMapper(_ => { }, typeof(BaseProfile));

        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

        builder.Services.Configure<TransientRetryPolicyOptions>(builder.Configuration.GetSection(TransientRetryPolicyOptions.SectionName));

        builder.Services.Configure<ManualCheckOptions>(builder.Configuration.GetSection(ManualCheckOptions.SectionName));

        builder.Services.Configure<EncryptionHashingOptions>(builder.Configuration.GetSection(EncryptionHashingOptions.SectionName));

        builder.Services.AddSingleton<ITransientRetryPolicyService, TransientRetryPolicyService>();
        builder.Services.AddSingleton<IEncryptionHashingService, EncryptionHashingService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IdentitySchemaInitializer>();
        builder.Services.AddScoped<ExternalAccountRegistration>();
        builder.Services.AddScoped<AuthenticatorMfa>();
        builder.Services.AddSingleton<MfaFlows>();
        builder.Services.AddScoped<IdentityRoleInitializer>();
        builder.Services.AddScoped<IScrapeHistoryDataService, ScrapeHistoryDataService>();
        builder.Services.AddScoped<IScrapeExecutionService, ScrapeExecutionService>();
        builder.Services.AddScoped<IManualCheckDataService, ManualCheckDataService>();
        builder.Services.AddScoped<IManualCheckListingSource, SteamManualCheckListingSource>();
        builder.Services.AddScoped<IManualCheckExecutionService, ManualCheckExecutionService>();
        builder.Services.AddScoped<IAutomaticQueueDataService, AutomaticQueueDataService>();
        builder.Services.AddScoped<IAutomationAccessService, AutomationAccessService>();
        builder.Services.AddSingleton<IManualCheckDelay, ManualCheckDelay>();
        builder.Services.AddScoped<IWishlistNotificationRecipientService, WishlistNotificationRecipientService>();
        builder.Services.AddScoped<IWishlistCheckExecutionService, WishlistCheckExecutionService>();
        builder.Services.AddScoped<ISteamRepository, SteamRepository>();
        builder.Services.AddScoped<ISteamService, SteamService>();
        builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
        builder.Services.AddScoped<IWishlistService, WishlistService>();

        builder.Services.AddSingleton<IManualCheckQueue, ManualCheckQueue>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHostedService<ManualCheckWorker>();
        builder.Services.AddHostedService<AutomaticQueueWorker>();
        builder.Services.AddHostedService<AutomationReconciliationWorker>();
        builder.Services.AddHttpClient(ManualCheckOptions.HttpClientName, client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        builder.Services.AddRabbitMqMessageBroker(builder.Configuration);

        builder.Services.AddMemoryCache();
        builder.Services.AddSteamAppDistributedCache(builder.Configuration);

        // Wishlist Job
        builder.Services.AddScoped<WishlistCheckJob>();
        builder.Services.AddHostedService<BackgroundWorkerService<WishlistCheckJob>>();
        builder.Services.Configure<WorkerOptions>(nameof(WishlistCheckJob), builder.Configuration.GetSection("Workers:WishlistCheck"));
    }
}
