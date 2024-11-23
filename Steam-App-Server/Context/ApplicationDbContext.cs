using Microsoft.EntityFrameworkCore;
using SteamAppServer.Models;

namespace SteamAppServer.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Quality> Qualities { get; set; }
    }
}
