namespace SteamApp.Infrastructure.DTOs.Item
{
    public class UpdateItemDto
    {
        public virtual long Id { get; set; }
        public virtual string? Name { get; set; }
        public virtual bool? IsActive { get; set; }
        public virtual bool? IsWeapon { get; set; }
        public virtual long? ClassId { get; set; }
        public virtual long? SlotId { get; set; }
    }
}
