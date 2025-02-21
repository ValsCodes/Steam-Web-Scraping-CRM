namespace SteamApp.Infrastructure
{
    public abstract class BaseResult
    {
        public string Error { get; set; }
        public string InternalCode { get; set; }
    }
}
