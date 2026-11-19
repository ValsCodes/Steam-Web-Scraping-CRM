using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class GameAddOnConfiguration : IEntityTypeConfiguration<GameAddOn>
{
    public void Configure(EntityTypeBuilder<GameAddOn> entity)
    {
        entity.ToTable("game_add_on");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.Price).HasColumnName("price");
    }
}
