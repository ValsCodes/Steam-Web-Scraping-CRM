using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.ValueObjects.Authentication;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Mapper;
using SteamApp.WebAPI.MinimalAPIs;
using SteamApp.WebAPI.Repositories;
using SteamApp.WebAPI.Services;
using System.Text;

namespace SteamApp.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        var clients = builder.Configuration
            .GetSection("Clients")
            .Get<List<ClientDefinition>>() ?? new List<ClientDefinition>();

        builder.Services.AddSingleton<IReadOnlyList<ClientDefinition>>(clients);

        var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
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

        builder.Services.AddAuthorization();

        builder.Services
            .AddControllers()
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

        builder.Services.AddCors(opts =>
        {
            opts.AddPolicy("AllowAllOrigins",
                policy => policy.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SteamApp API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }
                ] = Array.Empty<string>()
            });
        });

        builder.Services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddScoped<ISteamService, SteamService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IItemService, ItemService>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SteamApp API v1"));

        app.UseHttpsRedirection();

        app.UseCors("AllowAllOrigins");

        app.UseAuthentication();
        app.UseAuthorization();

        // Minimal API endpoints
        app.MapQualityEndpoints();
        app.MapClassEndpoints();
        app.MapSlotEndpoints();
        app.MapGradeEndpoints();
        app.MapSkinEndpoints();

        // Regular API controllers
        app.MapControllers();

        app.Run();
    }
}
