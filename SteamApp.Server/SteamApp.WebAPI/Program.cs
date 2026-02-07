using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using SteamApp.Domain.ValueObjects;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.Mapper;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.Repositories;
using SteamApp.WebAPI.Services;
using SteamApp.WebApiClient;
using SteamApp.WebApiClient.Managers;
using System.Text;

namespace SteamApp.WebAPI;

public class Program
{
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
        {
            "ConnectionStrings:DefaultConnection",
            "JwtSettings:Key",
            "JwtSettings:Issuer",
            "JwtSettings:Audience",
            "InternalClient:ClientSecret"
        };

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
            .Get<List<ClientDefinition>>() ?? new List<ClientDefinition>();

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

        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy("InternalJob", policy =>
            {
                policy.RequireClaim("scope", "internal");
            });
        });

        builder.Services
            .AddControllers()
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

        builder.Services.AddCors(opts =>
        {
            opts.AddPolicy("AllowAllOrigins", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
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

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }
                ] = Array.Empty<string>()
            });
        });

        builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        {
            opts.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddAutoMapper(typeof(GameMappingProfile).Assembly);

        builder.Services.AddHttpClient<AuthApiClient>(client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["InternalApi:BaseUrl"]);
        });

        builder.Services.AddHttpClient<BaseApiClient>(client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["InternalApi:BaseUrl"]);
        });

        builder.Services.AddHttpClient<SteamApiClient>(client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["InternalApi:BaseUrl"]);
        });

        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("Mailstrap"));

        builder.Services.AddScoped<ISteamRepository, SteamRepository>();
        builder.Services.AddScoped<ISteamService, SteamService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<SteamApiClient>();
        builder.Services.AddScoped<WishlistCheckJob>();

        builder.Services.AddHostedService<BackgroundWorkerService<WishlistCheckJob>>();

        builder.Services.Configure<WorkerOptions>(nameof(WishlistCheckJob),builder.Configuration.GetSection("Workers:WishlistCheck"));

        builder.Services.Configure<HostOptions>(o =>
        {
            o.BackgroundServiceExceptionBehavior =
                BackgroundServiceExceptionBehavior.Ignore;
            o.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "SteamApp API v1");
        });

        app.UseHttpsRedirection();
        app.UseCors("AllowAllOrigins");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGameEndpoints();
        app.MapGameUrlEndpoints();
        app.MapProductEndpoints();
        app.MapPixelEndpoints();
        app.MapWatchListEndpoints();
        app.MapWishListEndpoints();
        app.MapGameUrlProductsEndpoints();
        app.MapTagsEndpoints();
        app.MapProductTagsEndpoints();  

        app.Run();
    }
}