using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities
{
    public sealed class AutomatedScrapeHistory
    {
        public long Id { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string Endpoint { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ScrapeType { get; set; } = string.Empty;

        public long GameUrlId { get; set; }

        public short Page { get; set; }

        public string SetupJson { get; set; } = "{}";

        public string? ResultsJson { get; set; }

        public int ResultCount { get; set; }

        public DateTime Date { get; set; }

        public string? ErrorText { get; set; }

        public bool IsHaveError { get; set; }

        public ScrapeJobStatusEnum Status { get; set; } = ScrapeJobStatusEnum.Succeeded;

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        [MaxLength(64)]
        public string? CorrelationId { get; set; }
    }
}
