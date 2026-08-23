using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using Newtonsoft.Json.Serialization;
using SteamApp.Application.Mapper;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Repositories;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.Jobs.Base;
using SteamApp.WebAPI.ManualChecks;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.DependencyInjection;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;
using System.Net.Mail;
using System.Text;
using System.Threading.RateLimiting;

namespace SteamApp.WebAPI;

public class Program
{
    private const int MinJwtSigningKeyBytes = 32;
    private const int MaxJwtDurationMinutes = 120;
    private const int DatabaseMigrationMaxAttempts = 30;
    private static readonly TimeSpan DatabaseMigrationRetryDelay = TimeSpan.FromSeconds(2);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.Sources.Clear();

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        // Environment variables must remain the final provider so a launcher or
        // deployment can override user-secrets without modifying stored secrets.
        builder.Configuration.AddEnvironmentVariables();

        string[] required =
        [
            "ConnectionStrings:DefaultConnection",
            "JwtSettings:Key",
            "JwtSettings:Issuer",
            "JwtSettings:Audience",
            "JwtSettings:DurationMinutes"
        ];

        foreach (var key in required)
        {
            if (string.IsNullOrWhiteSpace(builder.Configuration[key]))
            {
                throw new InvalidOperationException($"Missing required configuration: {key}");
            }
        }

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        var jwt = builder.Configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;

        var clients = builder.Configuration
            .GetSection("Clients")
            .Get<List<ClientDefinition>>() ?? [];

        ValidateJwtSettings(jwt);
        ValidateClientDefinitions(clients, builder.Environment);
        ValidateHostFilteringConfiguration(builder.Configuration, builder.Environment);
        ValidateEmailConfiguration(builder.Configuration, builder.Environment);

        builder.Services.AddSingleton<IReadOnlyList<ClientDefinition>>(clients);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                var key = Encoding.UTF8.GetBytes(jwt.Key);

                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services
            .AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.User.RequireUniqueEmail = true;

                opts.Password.RequiredLength = 8;
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireNonAlphanumeric = false;

                opts.Lockout.AllowedForNewUsers = true;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthorization(opts =>
        {
            opts.DefaultPolicy = BuildApiAuthorizationPolicy();
            opts.FallbackPolicy = opts.DefaultPolicy;

            opts.AddPolicy(SecurityPolicies.ApiUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(
                    "scope",
                    SecurityPolicies.UserScope,
                    SecurityPolicies.InternalScope);
            });

            opts.AddPolicy(SecurityPolicies.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", SecurityPolicies.UserScope);
                policy.RequireRole(SecurityPolicies.AdminRole);
            });

            opts.AddPolicy(SecurityPolicies.InternalJob, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", SecurityPolicies.InternalScope);
            });
        });

        builder.Services
            .AddControllers()
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        ValidateCorsOrigins(allowedOrigins, builder.Environment);

        builder.Services.AddCors(opts =>
        {
            opts.AddPolicy("FrontendCors", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                        return;
                    }

                    throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");
                }

                policy.WithOrigins(allowedOrigins)
                      .WithMethods(
                          HttpMethods.Get,
                          HttpMethods.Post,
                          HttpMethods.Put,
                          HttpMethods.Patch,
                          HttpMethods.Delete)
                      .WithHeaders(
                          HeaderNames.Accept,
                          HeaderNames.Authorization,
                          HeaderNames.ContentType);
            });
        });

        builder.Services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = async (context, cancellationToken) =>
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;
                var retryAfter = context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out TimeSpan retryDelay)
                    ? retryDelay
                    : TimeSpan.Zero;

                if (retryAfter > TimeSpan.Zero)
                {
                    response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "Rate limit rejected {Method} {Path}. Trace ID: {TraceId}; retry after: {RetryAfter}.",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    context.HttpContext.TraceIdentifier,
                    retryAfter);

                await response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests",
                        Detail = retryAfter > TimeSpan.Zero
                            ? $"The request limit was reached. Try again in {Math.Ceiling(retryAfter.TotalSeconds)} seconds."
                            : "The request limit was reached. Wait briefly and try again.",
                        Instance = context.HttpContext.Request.Path,
                        Extensions =
                        {
                            ["traceId"] = context.HttpContext.TraceIdentifier
                        }
                    },
                    cancellationToken);
            };

            opts.AddPolicy(SecurityPolicies.AuthRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetRemoteAddressPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            opts.AddPolicy(SecurityPolicies.ApiRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrRemoteAddressPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 120,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            opts.AddPolicy(SecurityPolicies.ExpensiveApiRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrRemoteAddressPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SteamApp API",
                Version = "v2"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        builder.Services.AddDbContextFactory<ApplicationDbContext>(opts =>
        {
            opts.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(Program).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        });

        builder.Services.AddAutoMapper(_ => { }, typeof(BaseProfile));

        // Mailtrap setup
        //builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Mailtrap").Exists() 
        //    ? builder.Configuration.GetSection("Mailtrap") : builder.Configuration.GetSection("Mailstrap"));

        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

        builder.Services.Configure<TransientRetryPolicyOptions>(builder.Configuration.GetSection(TransientRetryPolicyOptions.SectionName));

        builder.Services.Configure<ManualCheckOptions>(builder.Configuration.GetSection(ManualCheckOptions.SectionName));

        builder.Services.Configure<EncryptionHashingOptions>(builder.Configuration.GetSection(EncryptionHashingOptions.SectionName));

        builder.Services.AddSingleton<ITransientRetryPolicyService, TransientRetryPolicyService>();
        builder.Services.AddSingleton<IEncryptionHashingService, EncryptionHashingService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IdentitySchemaInitializer>();
        builder.Services.AddScoped<IdentityRoleInitializer>();
        builder.Services.AddScoped<IScrapeHistoryDataService, ScrapeHistoryDataService>();
        builder.Services.AddScoped<IScrapeExecutionService, ScrapeExecutionService>();
        builder.Services.AddScoped<IManualCheckDataService, ManualCheckDataService>();
        builder.Services.AddScoped<IManualCheckExecutionService, ManualCheckExecutionService>();
        builder.Services.AddSingleton<IManualCheckDelay, ManualCheckDelay>();
        builder.Services.AddScoped<IWishlistNotificationRecipientService, WishlistNotificationRecipientService>();
        builder.Services.AddScoped<ISteamRepository, SteamRepository>();
        builder.Services.AddScoped<ISteamService, SteamService>();
        builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
        builder.Services.AddScoped<IWishlistService, WishlistService>();

        builder.Services.AddSingleton<IManualCheckQueue, ManualCheckQueue>();
        builder.Services.AddHostedService<ManualCheckWorker>();
        builder.Services.AddHttpClient(ManualCheckOptions.HttpClientName, client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamApp-ManualCheck/1.0");
        });

        builder.Services.AddRabbitMqMessageBroker(builder.Configuration);

        builder.Services.AddMemoryCache();
        AddDistributedCache(builder.Services, builder.Configuration);

        // Wishlist Job
        builder.Services.AddScoped<WishlistCheckJob>();
        builder.Services.AddHostedService<BackgroundWorkerService<WishlistCheckJob>>();
        builder.Services.Configure<WorkerOptions>(nameof(WishlistCheckJob), builder.Configuration.GetSection("Workers:WishlistCheck"));

        builder.Services.Configure<HostOptions>(o =>
        {
            o.BackgroundServiceExceptionBehavior =
                BackgroundServiceExceptionBehavior.StopHost;
            o.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            if (ShouldEnsureIdentitySchema(app.Configuration, app.Environment))
            {
                scope.ServiceProvider
                    .GetRequiredService<IdentitySchemaInitializer>()
                    .EnsureCreatedAsync()
                    .GetAwaiter()
                    .GetResult();
            }

            if (ShouldApplyMigrationsOnStartup(app.Configuration))
            {
                ApplyDatabaseMigrationsAsync(
                        scope.ServiceProvider,
                        app.Logger,
                        app.Lifetime.ApplicationStopping)
                    .GetAwaiter()
                    .GetResult();
            }

            scope.ServiceProvider
                .GetRequiredService<IdentityRoleInitializer>()
                .EnsureRolesAsync()
                .GetAwaiter()
                .GetResult();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "SteamApp API v1");
            });
        }
        else
        {
            app.UseHsts();
        }

        app.UseSecurityHeaders();
        app.UseHttpsRedirection();
        app.UseCors("FrontendCors");
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGameEndpoints();
        app.MapGameUrlEndpoints();
        app.MapScrapingModeEndpoints();
        app.MapProductEndpoints();
        app.MapPixelEndpoints();
        app.MapWatchListEndpoints();
        app.MapWishListEndpoints();
        app.MapFeedbackRequestEndpoints();
        app.MapGameUrlProductsEndpoints();
        app.MapTagsEndpoints();
        app.MapItemGroupsEndpoints();
        app.MapProductTagsEndpoints();
        app.MapGameUrlPixelsEndpoints();
        app.MapAdminUserEndpoints();

        app.Run();
    }

    private static AuthorizationPolicy BuildApiAuthorizationPolicy()
    {
        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireClaim(
                "scope",
                SecurityPolicies.UserScope,
                SecurityPolicies.InternalScope)
            .Build();
    }

    private static void ValidateJwtSettings(JwtSettings jwt)
    {
        var keyLength = Encoding.UTF8.GetByteCount(jwt.Key);

        if (keyLength < MinJwtSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"JwtSettings:Key must be at least {MinJwtSigningKeyBytes} bytes for HS256 signing.");
        }

        if (jwt.DurationMinutes <= 0 || jwt.DurationMinutes > MaxJwtDurationMinutes)
        {
            throw new InvalidOperationException(
                $"JwtSettings:DurationMinutes must be between 1 and {MaxJwtDurationMinutes}.");
        }
    }

    private static void ValidateClientDefinitions(
        IReadOnlyList<ClientDefinition> clients,
        IHostEnvironment environment)
    {
        var duplicateClientId = clients
            .Where(client => !string.IsNullOrWhiteSpace(client.ClientId))
            .GroupBy(client => client.ClientId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateClientId != null)
        {
            throw new InvalidOperationException(
                $"Duplicate client definition found for client id '{duplicateClientId.Key}'.");
        }

        foreach (var client in clients)
        {
            if (string.IsNullOrWhiteSpace(client.ClientId))
            {
                throw new InvalidOperationException("Client definitions must include ClientId.");
            }

            if (client.AllowedScope is not SecurityPolicies.UserScope and not SecurityPolicies.InternalScope)
            {
                throw new InvalidOperationException(
                    $"Client '{client.ClientId}' has unsupported AllowedScope '{client.AllowedScope}'.");
            }

            var hasPlainTextSecret = !string.IsNullOrWhiteSpace(client.ClientSecret);
            var hasHashedSecret = !string.IsNullOrWhiteSpace(client.ClientSecretHash);

            if (!hasPlainTextSecret && !hasHashedSecret)
            {
                throw new InvalidOperationException(
                    $"Client '{client.ClientId}' must define ClientSecretHash.");
            }

            if (hasHashedSecret && !IsSha256HexHash(client.ClientSecretHash!))
            {
                throw new InvalidOperationException(
                    $"Client '{client.ClientId}' ClientSecretHash must be a SHA-256 hex hash.");
            }

            if (!environment.IsDevelopment() && !hasHashedSecret)
            {
                throw new InvalidOperationException(
                    $"Client '{client.ClientId}' must use ClientSecretHash outside Development.");
            }
        }
    }

    private static void ValidateHostFilteringConfiguration(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (environment.IsDevelopment())
        {
            return;
        }

        var allowedHosts = configuration["AllowedHosts"];
        var hosts = allowedHosts?.Split(
            [';', ','],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        if (hosts.Length == 0 || hosts.Any(host => host == "*"))
        {
            throw new InvalidOperationException(
                "AllowedHosts must list explicit host names outside Development.");
        }
    }

    private static void ValidateCorsOrigins(
        IReadOnlyList<string> allowedOrigins,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(allowedOrigins);
        ArgumentNullException.ThrowIfNull(environment);

        if (environment.IsDevelopment() && allowedOrigins.Count == 0)
        {
            return;
        }

        foreach (var origin in allowedOrigins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                string.IsNullOrWhiteSpace(uri.Host))
            {
                throw new InvalidOperationException(
                    $"Cors:AllowedOrigins contains an invalid origin: '{origin}'.");
            }

            if (!environment.IsDevelopment() &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cors:AllowedOrigins must use HTTPS outside Development: '{origin}'.");
            }
        }
    }

    private static void ValidateEmailConfiguration(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!configuration.GetValue<bool>("Workers:WishlistCheck:Enabled"))
        {
            return;
        }

        var required = new[]
        {
            "Email:Host",
            "Email:UserName",
            "Email:Password",
            "Email:FromAddress"
        };

        foreach (var key in required)
        {
            if (string.IsNullOrWhiteSpace(configuration[key]))
            {
                throw new InvalidOperationException(
                    $"Missing required email configuration: {key}");
            }
        }

        var port = configuration.GetValue<int>("Email:Port");
        if (port is <= 0 or > 65535)
        {
            throw new InvalidOperationException(
                "Email:Port must be between 1 and 65535.");
        }

        try
        {
            _ = new MailAddress(configuration["Email:FromAddress"]!);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Email:FromAddress must be a valid email address.",
                exception);
        }

        if (!environment.IsDevelopment() &&
            configuration.GetValue<bool>("Email:AllowInvalidCertificate"))
        {
            throw new InvalidOperationException(
                "Email:AllowInvalidCertificate can only be enabled in Development.");
        }
    }

    private static bool IsSha256HexHash(string value)
    {
        return value.Length == 64 &&
               value.All(Uri.IsHexDigit);
    }

    private static bool ShouldEnsureIdentitySchema(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        return configuration.GetValue<bool?>("Database:EnsureIdentitySchemaOnStartup")
               ?? environment.IsDevelopment();
    }

    private static bool ShouldApplyMigrationsOnStartup(IConfiguration configuration)
    {
        return configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
    }

    private static void AddDistributedCache(
        IServiceCollection services,
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

    private static async Task ApplyDatabaseMigrationsAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        for (var attempt = 1; attempt <= DatabaseMigrationMaxAttempts; attempt++)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                await db.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < DatabaseMigrationMaxAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    exception,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {Delay}.",
                    attempt,
                    DatabaseMigrationMaxAttempts,
                    DatabaseMigrationRetryDelay);

                await Task.Delay(DatabaseMigrationRetryDelay, cancellationToken);
            }
        }
    }

    private static string GetRemoteAddressPartitionKey(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetUserOrRemoteAddressPartitionKey(HttpContext context)
    {
        return context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? GetRemoteAddressPartitionKey(context);
    }
}
