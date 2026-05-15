namespace SteamApp.Application.DTOs.WatchList
{
    public sealed class WatchListDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateOnly RegistrationDate { get; set; }
        public bool IsActive { get; set; }
    }
}
