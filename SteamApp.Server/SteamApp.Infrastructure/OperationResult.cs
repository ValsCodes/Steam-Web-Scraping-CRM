namespace SteamApp.Infrastructure
{
    public class OperationResult
    {
        public long Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
