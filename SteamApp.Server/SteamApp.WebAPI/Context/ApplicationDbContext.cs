using Microsoft.EntityFrameworkCore;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Item> ManualSearchItems { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Paint> Paints { get; set; }

    public DbSet<Sheen> Sheens { get; set; }

    public DbSet<Skin> Skins { get; set; }

    public DbSet<Class> Classes { get; set; }

    public DbSet<Slot> Slots { get; set; }

    public DbSet<Quality> Qualities { get; set; }

    public DbSet<ItemSkins> ItemSkins { get; set; }

    public DbSet<Grade> Grades { get; set; }
}
