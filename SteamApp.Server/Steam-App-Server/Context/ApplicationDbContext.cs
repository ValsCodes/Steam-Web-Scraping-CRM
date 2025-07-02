using Microsoft.EntityFrameworkCore;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Product> Products { get; set; }
}
