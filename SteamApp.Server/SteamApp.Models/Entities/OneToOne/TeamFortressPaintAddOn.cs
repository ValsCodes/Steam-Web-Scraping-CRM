using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.OneToOne
{
    [Table("team_fortress_paint_add_on")]
    public class TeamFortressPaintAddOn
    {
        [Key]
        [Column("game_add_on_id")]
        public long GameAddOnId { get; set; }

        [ForeignKey(nameof(GameAddOnId))]
        public GameAddOn GameAddOn { get; set; }

        [Column("r_value")]
        public byte R { get; set; }

        [Column("g_value")]
        public byte G { get; set; }

        [Column("b_value")]
        public byte B { get; set; }
    }
}
