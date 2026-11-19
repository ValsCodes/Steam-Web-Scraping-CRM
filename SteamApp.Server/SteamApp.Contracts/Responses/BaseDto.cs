namespace SteamApp.Contracts.Common
{
    public class BaseDto
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; } = string.Empty;
    }
}
