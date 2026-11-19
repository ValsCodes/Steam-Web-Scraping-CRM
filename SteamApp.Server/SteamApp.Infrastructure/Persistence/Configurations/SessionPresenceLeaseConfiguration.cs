using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class SessionPresenceLeaseConfiguration : IEntityTypeConfiguration<SessionPresenceLease>
{
    public void Configure(EntityTypeBuilder<SessionPresenceLease> entity)
    {
        entity.ToTable("session_presence_lease");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.TabId).HasColumnName("tab_id");
        entity.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        entity.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc");
    }
}
