using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    // Uses long as the default type for Id
    public class BaseModel : BaseModel<long>
    {
    }

    public abstract class BaseModel<T>
    {
        [Key]
        [Column("id")]
        public T Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
