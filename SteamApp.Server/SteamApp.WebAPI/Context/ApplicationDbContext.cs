using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<GameUrl> GameUrls { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Pixel> Pixels { get; set; }
    public DbSet<WatchList> WatchList { get; set; }
    public DbSet<WishList> WishLists { get; set; }
    public DbSet<GameUrlProducts> GameUrlsProducts { get; set; }
    public DbSet<GameUrlPixels> GameUrlsPixels { get; set; }
    public DbSet<GameAddOn> GameAddOns { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTags> ProductTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>();
        modelBuilder.Entity<GameUrl>();     
        modelBuilder.Entity<WatchList>();
        modelBuilder.Entity<WishList>();
        modelBuilder.Entity<GameAddOn>();

        modelBuilder.Entity<Pixel>(entity =>
        {
            entity.HasOne(p => p.Game)
                  .WithMany(g => g.Pixels)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Game)
                  .WithMany(g => g.Products)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameAddOn>(entity =>
        {
            entity.HasOne(a => a.Game)
                  .WithMany(g => g.GameAddOns)
                  .HasForeignKey(a => a.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameUrlPixels>(entity =>
        {
            entity.HasKey(e => new { e.PixelId, e.GameUrlId });

            entity.HasOne(e => e.Pixel)
                  .WithMany(p => p.GameUrlsPixels)
                  .HasForeignKey(e => e.PixelId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GameUrl)
                  .WithMany(g => g.GameUrlsPixels)
                  .HasForeignKey(e => e.GameUrlId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameUrlProducts>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.GameUrlId });

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.GameUrlsProducts)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GameUrl)
                  .WithMany(g => g.GameUrlsProducts)
                  .HasForeignKey(e => e.GameUrlId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductTags>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.TagId });

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductTags)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tag)
                  .WithMany(g => g.ProductTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasOne(p => p.Game)
                  .WithMany(g => g.Tags)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}