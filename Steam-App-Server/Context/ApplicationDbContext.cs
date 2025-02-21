using Microsoft.EntityFrameworkCore;
using SteamApp.Models;

namespace SteamApp.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Quality> Qualities { get; set; }
    }
}
