using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.Tag
{
    public class TagUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public bool? IsActive { get; set; }

        public long? ItemGroupId { get; set; }
    }
}
