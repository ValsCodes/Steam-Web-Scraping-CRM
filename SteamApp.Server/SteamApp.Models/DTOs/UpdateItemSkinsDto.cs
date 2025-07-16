namespace SteamApp.Models.DTOs;

public class UpdateItemSkinsDto : BaseUpdateDto
{
    public virtual long Id { get; set; }

    public virtual long? ItemId { get; set; }

    public virtual long? SkinId { get; set; }

    public virtual long? GradeId { get; set; }

}
