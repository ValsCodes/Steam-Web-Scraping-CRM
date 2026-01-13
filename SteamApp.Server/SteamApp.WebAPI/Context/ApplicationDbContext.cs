using Microsoft.EntityFrameworkCore;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<GameUrl> GameUrls { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ExtraPixel> ExtraPixels { get; set; }
    public DbSet<WatchListItem> WatchListItems { get; set; }
    public DbSet<WishListItem> WishListItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>();
        modelBuilder.Entity<GameUrl>();
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<ExtraPixel>();
        modelBuilder.Entity<WatchListItem>();
        modelBuilder.Entity<WishListItem>();
    }
}