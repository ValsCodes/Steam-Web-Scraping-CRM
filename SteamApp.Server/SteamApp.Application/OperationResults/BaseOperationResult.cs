namespace SteamApp.Models.OperationResults
{
    public class BaseOperationResult
    {
        public long Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
