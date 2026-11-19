using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ManualCheckConditionOperatorConfiguration : IEntityTypeConfiguration<ManualCheckConditionOperator>
{
    public void Configure(EntityTypeBuilder<ManualCheckConditionOperator> entity)
    {
        entity.ToTable("manual_check_condition_operator");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
    }
}
