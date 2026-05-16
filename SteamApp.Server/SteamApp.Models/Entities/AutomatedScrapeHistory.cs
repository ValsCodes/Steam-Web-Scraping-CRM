using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("automated_scrape_history")]
    public sealed class AutomatedScrapeHistory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [MaxLength(450)]
        [Column("user_id")]
        public string? UserId { get; set; }

        [MaxLength(100)]
        [Column("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("scrape_type")]
        public string ScrapeType { get; set; } = string.Empty;

        [Column("game_url_id")]
        public long GameUrlId { get; set; }

        [Column("page")]
        public short Page { get; set; }

        [Column("setup_json")]
        public string SetupJson { get; set; } = "{}";

        [Column("results_json")]
        public string? ResultsJson { get; set; }

        [Column("result_count")]
        public int ResultCount { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("error_text")]
        public string? ErrorText { get; set; }

        [Column("is_have_error")]
        public bool IsHaveError { get; set; }
    }
}
