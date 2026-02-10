namespace SteamApp.Application.DTOs.WatchList
{
    public sealed class WatchListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public bool IsActive { get; set; }
    }
}
