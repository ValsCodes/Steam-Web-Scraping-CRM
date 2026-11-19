using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ItemGroupConfiguration : IEntityTypeConfiguration<ItemGroup>
{
    public void Configure(EntityTypeBuilder<ItemGroup> entity)
    {
        entity.ToTable("item_group");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
