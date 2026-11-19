using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ScrapingModeConfiguration : IEntityTypeConfiguration<ScrapingMode>
{
    public void Configure(EntityTypeBuilder<ScrapingMode> entity)
    {
        entity.ToTable("scraping_mode");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
    }
}
