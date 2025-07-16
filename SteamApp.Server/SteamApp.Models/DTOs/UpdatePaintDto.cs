namespace SteamApp.Models.DTOs;

public class UpdatePaintDto : BaseUpdateDto
{
    public virtual short Id { get; set; }

    public virtual byte? R { get; set; }

    public virtual byte? G { get; set; }

    public virtual byte? B { get; set; }

    public virtual bool? IsGoodPaint { get; set; }
}
