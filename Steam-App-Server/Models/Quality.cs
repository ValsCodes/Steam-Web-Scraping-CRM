using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamAppServer.Models
{
    [Table("quality")]
    public class Quality
    {
        [Key]
        [Column("id")]
        public virtual short Id { get; set; }

        [Column("name")]
        public virtual string Name { get; set; }
    }
}
