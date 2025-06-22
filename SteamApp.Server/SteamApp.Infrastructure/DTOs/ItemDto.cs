using SteamApp.Infrastructure.Models;

namespace SteamApp.Infrastructure.DTOs
{
    public class ItemDto : IItem
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsWeapon { get; set; }
    }
}
