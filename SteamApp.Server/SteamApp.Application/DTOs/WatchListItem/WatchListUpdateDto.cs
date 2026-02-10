namespace SteamApp.Application.DTOs.WatchListItem
{
    public sealed class WatchListUpdateDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public DateOnly? RegistrationDate { get; set; }
        public string? Url { get; set; }
        public bool? IsActive { get; set; }
    }
}
