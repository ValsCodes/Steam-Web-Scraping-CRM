using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;

namespace SteamApp.IntegrationTests.Support;

public static class IntegrationSeed
{
    public const string UserId = "integration-user-id";
    public const string UserEmail = "user@example.com";
    public const string UserName = "integration-user";
    public const string UserPassword = "Password1";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        await SeedIdentityAsync(scope.ServiceProvider);
        SeedDomain(db);
        await db.SaveChangesAsync();
    }

    private static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(SecurityPolicies.UserRole))
        {
            await roleManager.CreateAsync(new IdentityRole(SecurityPolicies.UserRole));
        }

        if (!await roleManager.RoleExistsAsync(SecurityPolicies.AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(SecurityPolicies.AdminRole));
        }

        var user = new ApplicationUser
        {
            Id = UserId,
            UserName = UserName,
            Email = UserEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, UserPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, SecurityPolicies.UserRole);
        await userManager.AddToRoleAsync(user, SecurityPolicies.AdminRole);
    }

    private static void SeedDomain(ApplicationDbContext db)
    {
        db.Games.AddRange(
            new Game
            {
                Id = 1,
                Name = "Alpha Game",
                BaseUrl = "https://steam.example/alpha",
                PageUrl = "https://steam.example/app/1",
                InternalId = 10,
                IsActive = true,
                UserId = UserId
            },
            new Game
            {
                Id = 2,
                Name = "Beta Game",
                BaseUrl = "https://steam.example/beta",
                PageUrl = "https://steam.example/app/2",
                InternalId = 20,
                IsActive = false,
                UserId = UserId
            },
            new Game
            {
                Id = 3,
                Name = "Disposable Game",
                BaseUrl = "https://steam.example/disposable",
                PageUrl = "https://steam.example/app/3",
                InternalId = 30,
                IsActive = true,
                UserId = UserId
            });

        db.GameUrls.AddRange(
            new GameUrl
            {
                Id = 1,
                Name = "Batch URL",
                GameId = 1,
                ScrapingModeId = (long)ScrapingModeEnum.Batch,
                PartialUrl = "https://steam.example/search?p={0}",
                StartPage = 1,
                EndPage = 5,
                IsActive = true,
                UserId = UserId
            },
            new GameUrl
            {
                Id = 2,
                Name = "Pixel URL",
                GameId = 1,
                ScrapingModeId = (long)ScrapingModeEnum.PixelBatch,
                PartialUrl = "https://steam.example/pixels?p={0}",
                PixelX = 4,
                PixelY = 5,
                PixelImageWidth = 62,
                PixelImageHeight = 62,
                IsActive = true,
                UserId = UserId
            });

        db.Products.AddRange(
            new Product
            {
                Id = 1,
                GameId = 1,
                Name = "Rocket Launcher",
                Rating = 5,
                IsActive = true,
                UserId = UserId
            },
            new Product
            {
                Id = 2,
                GameId = 2,
                Name = "Medigun",
                Rating = 3,
                IsActive = false,
                UserId = UserId
            });

        db.Pixels.AddRange(
            new Pixel
            {
                Id = 1,
                GameId = 1,
                Name = "Team Spirit",
                RedValue = 195,
                GreenValue = 108,
                BlueValue = 45,
                IsActive = true,
                UserId = UserId
            },
            new Pixel
            {
                Id = 2,
                GameId = 1,
                Name = "After Eight",
                RedValue = 45,
                GreenValue = 48,
                BlueValue = 44,
                IsActive = false,
                UserId = UserId
            });

        db.Tags.AddRange(
            new Tag { Id = 1, GameId = 1, Name = "Primary", IsActive = true, UserId = UserId },
            new Tag { Id = 2, GameId = 2, Name = "Support", IsActive = false, UserId = UserId });

        db.WishLists.AddRange(
            new WishList
            {
                Id = 1,
                GameId = 1,
                Name = "Cheap Alpha",
                Price = 9.99,
                IsActive = true,
                UserId = UserId
            },
            new WishList
            {
                Id = 2,
                GameId = 2,
                Name = "Cheap Beta",
                Price = 4.99,
                IsActive = false,
                UserId = UserId
            });

        db.WatchList.AddRange(
            new WatchList
            {
                Id = 1,
                Name = "Watch Alpha",
                Url = "https://steam.example/watch/1",
                RegistrationDate = new DateOnly(2026, 1, 1),
                IsActive = true,
                UserId = UserId
            },
            new WatchList
            {
                Id = 2,
                Name = "Watch Beta",
                Url = "https://steam.example/watch/2",
                RegistrationDate = new DateOnly(2026, 2, 1),
                IsActive = false,
                UserId = UserId
            });

        db.ProductTags.Add(new ProductTags { ProductId = 1, TagId = 1 });
        db.GameUrlsProducts.Add(new GameUrlProducts { ProductId = 1, GameUrlId = 1 });
        db.GameUrlsPixels.Add(new GameUrlPixels { PixelId = 1, GameUrlId = 2 });
    }
}
