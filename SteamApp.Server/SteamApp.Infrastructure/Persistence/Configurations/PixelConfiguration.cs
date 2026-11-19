using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class PixelConfiguration : IEntityTypeConfiguration<Pixel>
{
    public void Configure(EntityTypeBuilder<Pixel> entity)
    {
        entity.ToTable("pixel");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.RedValue).HasColumnName("r_value");
        entity.Property(x => x.GreenValue).HasColumnName("g_value");
        entity.Property(x => x.BlueValue).HasColumnName("b_value");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
