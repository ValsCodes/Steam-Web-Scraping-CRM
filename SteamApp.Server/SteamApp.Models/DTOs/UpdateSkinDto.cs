namespace SteamApp.Models.DTOs;

public class UpdateSkinDto : BaseUpdateDto
{
    public virtual bool? IsWarPaint { get; set; }

    public virtual short? QualityId { get; set; }

    public virtual bool? IsActive { get; set; }
}
