using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("pixel")]
    public sealed class Pixel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("game_id")]
        [ForeignKey(nameof(Game))]
        public long GameId { get; set; }
        [InverseProperty(nameof(Game.Pixels))]
        public Game Game { get; set; }

        [Column("r_value")]
        public long RedValue { get; set; }

        [Column("g_value")]
        public long GreenValue { get; set; }

        [Column("b_value")]
        public long BlueValue { get; set; }

        public ICollection<GameUrlPixels> GameUrlsPixels { get; set; } = [];
    }
}
