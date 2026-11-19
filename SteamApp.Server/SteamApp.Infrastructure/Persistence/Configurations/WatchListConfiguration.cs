using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class WatchListConfiguration : IEntityTypeConfiguration<WatchList>
{
    public void Configure(EntityTypeBuilder<WatchList> entity)
    {
        entity.ToTable("watch_list");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.GameUrlId).HasColumnName("GameUrlId");
        entity.Property(x => x.ProductId).HasColumnName("ProductId");
        entity.Property(x => x.Url).HasColumnName("url");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.RegistrationDate).HasColumnName("rd");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
