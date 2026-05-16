using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using Newtonsoft.Json.Serialization;
using SteamApp.Application.Mapper;
using SteamApp.Application.Services;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Repositories;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.Jobs.Base;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;
using System.Text;
using System.Threading.RateLimiting;

namespace SteamApp.WebAPI;

public class Program
{
    private const int MinJwtSigningKeyBytes = 32;
    private const int MaxJwtDurationMinutes = 120;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.Sources.Clear();

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

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
                sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
        });

        builder.Services.AddAutoMapper(_ => { }, typeof(BaseProfile));

        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("Mailtrap").Exists()
                ? builder.Configuration.GetSection("Mailtrap")
                : builder.Configuration.GetSection("Mailstrap"));

        builder.Services.Configure<TransientRetryPolicyOptions>(
            builder.Configuration.GetSection(TransientRetryPolicyOptions.SectionName));
        builder.Services.Configure<EncryptionHashingOptions>(
            builder.Configuration.GetSection(EncryptionHashingOptions.SectionName));

        builder.Services.AddSingleton<ITransientRetryPolicyService, TransientRetryPolicyService>();
        builder.Services.AddSingleton<IEncryptionHashingService, EncryptionHashingService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IdentitySchemaInitializer>();
        builder.Services.AddScoped<IScrapeHistoryDataService, ScrapeHistoryDataService>();
        builder.Services.AddScoped<IWishlistNotificationRecipientService, WishlistNotificationRecipientService>();
        builder.Services.AddScoped<ISteamRepository, SteamRepository>();
        builder.Services.AddScoped<ISteamService, SteamService>();
        builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
        builder.Services.AddScoped<IWishlistService, WishlistService>();

        builder.Services.AddMemoryCache();

        // Wishlist Job
        builder.Services.AddScoped<WishlistCheckJob>();
        builder.Services.AddHostedService<BackgroundWorkerService<WishlistCheckJob>>();
        builder.Services.Configure<WorkerOptions>(nameof(WishlistCheckJob),builder.Configuration.GetSection("Workers:WishlistCheck"));

        builder.Services.Configure<HostOptions>(o =>
        {
            o.BackgroundServiceExceptionBehavior =
                BackgroundServiceExceptionBehavior.StopHost;
            o.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        var app = builder.Build();

        if (ShouldEnsureIdentitySchema(app.Configuration, app.Environment))
        {
            using var scope = app.Services.CreateScope();

            scope.ServiceProvider
                .GetRequiredService<IdentitySchemaInitializer>()
                .EnsureCreatedAsync()
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
        app.MapGameUrlProductsEndpoints();
        app.MapTagsEndpoints();
        app.MapProductTagsEndpoints();
        app.MapGameUrlPixelsEndpoints();

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
