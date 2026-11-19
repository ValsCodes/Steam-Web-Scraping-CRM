using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities
{
    public sealed class WishList
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public long GameId { get; set; }
        public Game Game { get; set; } = null!;

        public double? Price { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }
    }
}
