using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities
{
    [Table("game_add_on")]
    public class GameAddOn
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("game_id")]
        [ForeignKey(nameof(Game))]
        public long GameId { get; set; }
        [InverseProperty(nameof(Game.GameAddOns))]
        public Game Game { get; set; }

        [Column("price")]
        public double? Price { get; set; }
    }
}
