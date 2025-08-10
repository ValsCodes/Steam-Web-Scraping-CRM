using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    public class AddOnType : BaseModel<short>
    {
        [Column("is_good")]
        public bool IsGood { get; set; }

        [Column("is_team_fortress_paint")]
        public bool IsTeamFortressPaint { get; set; }
    }
}
