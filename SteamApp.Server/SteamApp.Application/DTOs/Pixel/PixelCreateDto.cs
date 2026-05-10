namespace SteamApp.Application.DTOs.Pixel
{
    public sealed class PixelCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public long RedValue { get; set; }
        public long GreenValue { get; set; }
        public long BlueValue { get; set; }
        public long GameId { get; set; }
        public bool IsActive { get; set; }
    }
}
