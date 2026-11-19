using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ManualCheckRunConfiguration : IEntityTypeConfiguration<ManualCheckRun>
{
    public void Configure(EntityTypeBuilder<ManualCheckRun> entity)
    {
        entity.ToTable("manual_check_run");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.ManualCheckPresetId).HasColumnName("manual_check_preset_id");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
        entity.Property(x => x.PresetName).HasColumnName("preset_name");
        entity.Property(x => x.SetupJson).HasColumnName("setup_json");
        entity.Property(x => x.ResultsJson).HasColumnName("results_json");
        entity.Property(x => x.TotalProducts).HasColumnName("total_products");
        entity.Property(x => x.CheckedProducts).HasColumnName("checked_products");
        entity.Property(x => x.MatchedProducts).HasColumnName("matched_products");
        entity.Property(x => x.FailedProducts).HasColumnName("failed_products");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.PauseReason).HasColumnName("pause_reason");
        entity.Property(x => x.Date).HasColumnName("date");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        entity.Property(x => x.PausedAtUtc).HasColumnName("paused_at_utc");
        entity.Property(x => x.AccumulatedPausedMilliseconds).HasColumnName("accumulated_paused_milliseconds");
        entity.Property(x => x.ErrorText).HasColumnName("error_text");
        entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
    }
}
