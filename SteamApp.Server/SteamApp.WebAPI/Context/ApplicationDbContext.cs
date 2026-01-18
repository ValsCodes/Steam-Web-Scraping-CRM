using Microsoft.EntityFrameworkCore;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<GameUrl> GameUrls { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Pixel> Pixels { get; set; }
    public DbSet<WatchList> WatchList { get; set; }
    public DbSet<WishList> WishList { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>();
        modelBuilder.Entity<GameUrl>();
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<Pixel>();
        modelBuilder.Entity<WatchList>();
        modelBuilder.Entity<WishList>();
    }
}