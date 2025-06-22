using Microsoft.EntityFrameworkCore;
using SteamApp.Models;
using SteamApp.Models.Models;

namespace SteamApp.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
