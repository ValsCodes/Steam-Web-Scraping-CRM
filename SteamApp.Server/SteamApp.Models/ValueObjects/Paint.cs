using System.Drawing;

namespace SteamApp.Models.ValueObjects;

public class Paint
{
    public string Name { get; set; }

    public Color Color { get; set; }

    public bool IsGoodPaint { get; set; }
}
