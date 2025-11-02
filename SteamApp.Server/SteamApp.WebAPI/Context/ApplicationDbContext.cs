using Microsoft.EntityFrameworkCore;
using SteamApp.Models.Entities;
using SteamApp.Models.Entities.ManyToMany;
using SteamApp.Models.Entities.OneToOne;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    public DbSet<AddOnType> AddOnTypes { get; set; }
    public DbSet<Item> Item { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Skin> Skins { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<Quality> Qualities { get; set; }
    public DbSet<Target> Targets { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameUrl> GameUrls { get; set; }
    public DbSet<GameAddOn> GameAddOns { get; set; }
    public DbSet<WatchItem> WatchItems { get; set; }

    // One-to-One Relationships
    public DbSet<TeamFortressItem> TeamFortressItems { get; set; }
    public DbSet<TeamFortressPaintAddOn> TeamFortressPaintAddOns { get; set; }

    // M2M Relationships
    public DbSet<ItemGameAddOns> ItemsGameAddOns { get; set; }
    public DbSet<ItemQualities> ItemsQualities { get; set; }
    public DbSet<ItemSlots> ItemsSlots { get; set; }
    public DbSet<ItemClasses> ItemsClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ItemClasses>(e =>
        {
            e.HasKey(x => new { x.ItemId, x.ClassId });

            e.HasOne(x => x.Item)
                .WithMany(i => i.ItemClasses)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Class)
                .WithMany(c => c.ItemClasses)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ItemGameAddOns>(e =>
        {
            e.HasKey(x => new { x.ItemId, x.GameAddOnId });

            e.HasOne(x => x.Item)
                .WithMany(i => i.ItemGameAddOns)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.GameAddOn)
                .WithMany(c => c.ItemGameAddOns)
                .HasForeignKey(x => x.GameAddOnId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ItemSlots>(e =>
        {
            e.HasKey(x => new { x.ItemId, x.SlotId });

            e.HasOne(x => x.Item)
                .WithMany(i => i.ItemSlots)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
    public DbSet<Product> Products { get; set; }

            e.HasOne(x => x.Slot)
                .WithMany(c => c.ItemSlots)
                .HasForeignKey(x => x.SlotId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ItemQualities>(e =>
        {
            e.HasKey(x => new { x.ItemId, x.QualityId });

            e.HasOne(x => x.Item)
                .WithMany(i => i.ItemQualities)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Quality)
                .WithMany(c => c.ItemQualities)
                .HasForeignKey(x => x.QualityId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // GameUrl -> Game (once)
        modelBuilder.Entity<GameUrl>()
            .HasOne(gu => gu.Game)
            .WithMany(g => g.GameUrls)
            .HasForeignKey(gu => gu.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item -> Game (cascade)
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Game)
            .WithMany(g => g.Items)
            .HasForeignKey(i => i.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item -> GameUrl (no action to avoid multiple paths)
        modelBuilder.Entity<Item>()
            .HasOne(i => i.GameUrl)
            .WithMany(gu => gu.Items)
            .HasForeignKey(i => i.GameUrlId)
            .OnDelete(DeleteBehavior.NoAction);

        // Decimal precision (silence warnings)
        modelBuilder.Entity<GameAddOn>().Property(p => p.AddedValue).HasPrecision(18, 2);
        modelBuilder.Entity<Target>().Property(p => p.TargetPrice).HasPrecision(18, 2);
        modelBuilder.Entity<WatchItem>().Property(p => p.Price).HasPrecision(18, 2);
    }
}
