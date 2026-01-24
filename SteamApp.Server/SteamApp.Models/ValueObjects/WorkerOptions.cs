namespace SteamApp.Domain.ValueObjects
{
    public sealed class WorkerOptions
    {
        public bool Enabled { get; set; } = true;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
        public string? DisplayName { get; set; } // for logs/metrics
    }
}
