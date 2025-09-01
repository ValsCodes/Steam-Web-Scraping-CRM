namespace SteamApp.Models.DTOs.Class
{
    public sealed class ClassDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public long GameId { get; set; }
    }
}
