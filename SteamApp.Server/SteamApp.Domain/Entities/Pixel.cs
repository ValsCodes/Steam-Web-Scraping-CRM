using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities
{
    public sealed class Pixel
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public long GameId { get; set; }
        public Game Game { get; set; } = null!;

        public long RedValue { get; set; }

        public long GreenValue { get; set; }

        public long BlueValue { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public ICollection<GameUrlPixels> GameUrlsPixels { get; set; } = [];
    }
}
