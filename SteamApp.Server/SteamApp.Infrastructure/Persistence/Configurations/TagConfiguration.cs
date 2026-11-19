using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> entity)
    {
        entity.ToTable("tag");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.ItemGroupId).HasColumnName("item_group_id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
