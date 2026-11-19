using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class WishListConfiguration : IEntityTypeConfiguration<WishList>
{
    public void Configure(EntityTypeBuilder<WishList> entity)
    {
        entity.ToTable("wish_list");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.Price).HasColumnName("price");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.UserId).HasColumnName("user_id");

        entity.HasOne(x => x.Game)
              .WithMany(x => x.WishLists)
              .HasForeignKey(x => x.GameId);
    }
}
