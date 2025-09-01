namespace SteamApp.Models.DTOs.Skin;

public class SkinUpdateDto : BaseUpdateDto
{
    public virtual bool? IsWarPaint { get; set; }

    public virtual short? QualityId { get; set; }

    public virtual bool? IsActive { get; set; }
}
