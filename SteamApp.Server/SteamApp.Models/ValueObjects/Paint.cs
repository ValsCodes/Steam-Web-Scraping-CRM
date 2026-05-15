using System.Drawing;

namespace SteamApp.Domain.ValueObjects;

public class Paint
{
    public string Name { get; set; } = string.Empty;

    public Color Color { get; set; }

    public bool IsGoodPaint { get; set; }
}
