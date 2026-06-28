using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Identity;

namespace SteamApp.Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
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
    public DbSet<ScrapingMode> ScrapingModes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTags> ProductTags { get; set; }
    public DbSet<AutomatedScrapeHistory> AutomatedScrapeHistories { get; set; }
    public DbSet<FeedbackRequest> FeedbackRequests { get; set; }
    public DbSet<FeedbackRequestHistory> FeedbackRequestHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(ConfigureUserOwnedEntity);
        modelBuilder.Entity<GameUrl>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

            entity.HasOne(g => g.ScrapingMode)
                  .WithMany(s => s.GameUrls)
                  .HasForeignKey(g => g.ScrapingModeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WishList>(ConfigureUserOwnedEntity);
        modelBuilder.Entity<FeedbackRequest>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

            entity.Property(x => x.Type)
                  .HasColumnName("type");

            entity.Property(x => x.Title)
                  .HasMaxLength(FeedbackRequest.TitleMaxLength)
                  .HasColumnName("title");

            entity.Property(x => x.Description)
                  .HasMaxLength(FeedbackRequest.DescriptionMaxLength)
                  .HasColumnName("description");

            entity.Property(x => x.Area)
                  .HasMaxLength(FeedbackRequest.AreaMaxLength)
                  .HasColumnName("area");

            entity.Property(x => x.Status)
                  .HasColumnName("status");

            entity.Property(x => x.CreatedAtUtc)
                  .HasColumnName("created_at_utc");

            entity.Property(x => x.UpdatedAtUtc)
                  .HasColumnName("updated_at_utc");

            entity.Property(x => x.StatusChangedAtUtc)
                  .HasColumnName("status_changed_at_utc");

            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        });
        modelBuilder.Entity<FeedbackRequestHistory>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

            entity.Property(x => x.Action)
                  .HasColumnName("action");

            entity.Property(x => x.CreatedAtUtc)
                  .HasColumnName("created_at_utc");

            entity.Property(x => x.PreviousType)
                  .HasColumnName("previous_type");

            entity.Property(x => x.NewType)
                  .HasColumnName("new_type");

            entity.Property(x => x.PreviousTitle)
                  .HasMaxLength(FeedbackRequest.TitleMaxLength)
                  .HasColumnName("previous_title");

            entity.Property(x => x.NewTitle)
                  .HasMaxLength(FeedbackRequest.TitleMaxLength)
                  .HasColumnName("new_title");

            entity.Property(x => x.PreviousDescription)
                  .HasMaxLength(FeedbackRequest.DescriptionMaxLength)
                  .HasColumnName("previous_description");

            entity.Property(x => x.NewDescription)
                  .HasMaxLength(FeedbackRequest.DescriptionMaxLength)
                  .HasColumnName("new_description");

            entity.Property(x => x.PreviousArea)
                  .HasMaxLength(FeedbackRequest.AreaMaxLength)
                  .HasColumnName("previous_area");

            entity.Property(x => x.NewArea)
                  .HasMaxLength(FeedbackRequest.AreaMaxLength)
                  .HasColumnName("new_area");

            entity.Property(x => x.PreviousStatus)
                  .HasColumnName("previous_status");

            entity.Property(x => x.NewStatus)
                  .HasColumnName("new_status");

            entity.HasOne(x => x.FeedbackRequest)
                  .WithMany()
                  .HasForeignKey(x => x.FeedbackRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.FeedbackRequestId, x.CreatedAtUtc });
        });
        modelBuilder.Entity<GameAddOn>();
        modelBuilder.Entity<ScrapingMode>();
        modelBuilder.Entity<AutomatedScrapeHistory>(entity =>
        {
            entity.Property(x => x.UserId)
                  .HasMaxLength(450)
                  .HasColumnName("user_id");

            entity.Property(x => x.Endpoint)
                  .HasMaxLength(100)
                  .HasColumnName("endpoint");

            entity.Property(x => x.ScrapeType)
                  .HasMaxLength(50)
                  .HasColumnName("scrape_type");

            entity.Property(x => x.SetupJson)
                  .HasColumnName("setup_json");

            entity.Property(x => x.ResultsJson)
                  .HasColumnName("results_json");

            entity.Property(x => x.ErrorText)
                  .HasColumnName("error_text");

            entity.HasIndex(x => new { x.UserId, x.Date });
            entity.HasIndex(x => x.GameUrlId);
        });
        modelBuilder.Entity<WatchList>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

            entity.HasOne(w => w.GameUrl)
                  .WithMany(g => g.WatchLists)
                  .HasForeignKey(w => w.GameUrlId);

            entity.HasOne(w => w.Product)
                  .WithMany(p => p.WatchLists)
                  .HasForeignKey(w => w.ProductId);
        });

        modelBuilder.Entity<ScrapingMode>().HasData(
            new ScrapingMode { Id = (long)ScrapingModeEnum.ManualBatch, Name = "Manual Batch" },
            new ScrapingMode { Id = (long)ScrapingModeEnum.Batch, Name = "Batch" },
            new ScrapingMode { Id = (long)ScrapingModeEnum.PixelBatch, Name = "Pixel Batch" },
            new ScrapingMode { Id = (long)ScrapingModeEnum.PublicApi, Name = "Public API" });

        modelBuilder.Entity<Pixel>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

            entity.HasOne(p => p.Game)
                  .WithMany(g => g.Pixels)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            ConfigureUserOwnedEntity(entity);

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
            ConfigureUserOwnedEntity(entity);

            entity.HasOne(p => p.Game)
                  .WithMany(g => g.Tags)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureUserOwnedEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.Property<string?>("UserId")
              .HasMaxLength(450)
              .HasColumnName("user_id");

        entity.HasIndex("UserId");

        entity.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey("UserId")
              .OnDelete(DeleteBehavior.SetNull);
    }
}
