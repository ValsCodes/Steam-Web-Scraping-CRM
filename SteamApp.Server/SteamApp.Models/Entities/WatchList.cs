using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("watch_list")]
    public sealed class WatchList
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("GameUrlId")]
        [ForeignKey(nameof(GameUrl))]
        public long? GameUrlId { get; set; }
        public GameUrl? GameUrl { get; set; }

        [Column("ProductId")]
        [ForeignKey(nameof(Product))]
        public long? ProductId { get; set; }
        public Product? Product { get; set; }

        [Column("url")]
        public string? Url { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("rd")]
        public DateOnly RegistrationDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [MaxLength(450)]
        [Column("user_id")]
        public string? UserId { get; set; }
    }
}
