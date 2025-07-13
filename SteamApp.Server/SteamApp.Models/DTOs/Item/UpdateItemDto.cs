namespace SteamApp.Models.DTOs.Item;

public class UpdateItemDto
{
    public virtual long Id { get; set; }
    public virtual string? Name { get; set; }
    public virtual bool? IsActive { get; set; }
    public virtual bool? IsWeapon { get; set; }
    public virtual long? ClassId { get; set; }
    public virtual long? SlotId { get; set; }
    public virtual int? CurrentStock { get; set; }
    public virtual int? TradesCount { get; set; }
    public virtual short? Rating { get; set; }
}
