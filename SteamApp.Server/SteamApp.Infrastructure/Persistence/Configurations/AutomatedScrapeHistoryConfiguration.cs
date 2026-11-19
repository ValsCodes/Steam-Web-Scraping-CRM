using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomatedScrapeHistoryConfiguration : IEntityTypeConfiguration<AutomatedScrapeHistory>
{
    public void Configure(EntityTypeBuilder<AutomatedScrapeHistory> entity)
    {
        entity.ToTable("automated_scrape_history");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.Endpoint).HasColumnName("endpoint");
        entity.Property(x => x.ScrapeType).HasColumnName("scrape_type");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
        entity.Property(x => x.Page).HasColumnName("page");
        entity.Property(x => x.SetupJson).HasColumnName("setup_json");
        entity.Property(x => x.ResultsJson).HasColumnName("results_json");
        entity.Property(x => x.ResultCount).HasColumnName("result_count");
        entity.Property(x => x.Date).HasColumnName("date");
        entity.Property(x => x.ErrorText).HasColumnName("error_text");
        entity.Property(x => x.IsHaveError).HasColumnName("is_have_error");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
    }
}
