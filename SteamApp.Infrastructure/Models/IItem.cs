namespace SteamApp.Infrastructure.Models
{
    public interface IItem
    {
        long Id { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
        bool IsWeapon { get; set; }
    }
}
