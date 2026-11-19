using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities
{
    public class Game
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? BaseUrl { get; set; }

        public string? PageUrl { get; set; }

        public long? InternalId { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public ICollection<GameUrl> GameUrls { get; set; } = [];
        public ICollection<WishList> WishLists{ get; set; } = [];
        public ICollection<Product> Products { get; set; } = [];
        public ICollection<Pixel> Pixels { get; set; } = [];
        public ICollection<GameAddOn> GameAddOns { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];
        public ICollection<ItemGroup> ItemGroups { get; set; } = [];
    }
}
