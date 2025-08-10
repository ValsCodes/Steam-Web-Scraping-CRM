using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game")]
    public class Game : BaseModel
    {
        public ICollection<Item> Items { get; set; } = [];
        public ICollection<GameUrl> GameUrls { get; set; } = [];
        public ICollection<GameAddOn> GameAddOns { get; set; } = [];
        public ICollection<Grade> Grades { get; set; } = [];
        public ICollection<Invoice> Invoices { get; set; } = [];
        public ICollection<Class> Classes { get; set; } = [];
        public ICollection<Quality> Qualities { get; set; } = [];
    }
}
