namespace SteamApp.Models.DTOs;

public class UpdateSheenDto : BaseUpdateDto
{
    public virtual short Id { get; set; }
    public virtual bool? IsGoodSheen { get; set; }
}
