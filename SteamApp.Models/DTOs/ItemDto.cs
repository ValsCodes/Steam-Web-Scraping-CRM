namespace SteamApp.Models.DTOs
{
    public class ItemDto
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual bool IsWeapon { get; set; }
    }
}
