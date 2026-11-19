using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities
{
    public sealed class WatchList
    {
        public long Id { get; set; }

        public long? GameUrlId { get; set; }
        public GameUrl? GameUrl { get; set; }

        public long? ProductId { get; set; }
        public Product? Product { get; set; }

        public string? Url { get; set; }

        public string? Name { get; set; }

        public DateOnly RegistrationDate { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }
    }
}
