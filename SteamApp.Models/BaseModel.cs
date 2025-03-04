using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models
{
    public class BaseModel
    {
        [Key]
        [Column("id")]
        public virtual long Id { get; set; }

        [Column("name")]
        public virtual string Name { get; set; }
    }
}
