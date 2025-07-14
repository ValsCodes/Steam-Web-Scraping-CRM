using SteamApp.Models.ValueObjects;
using System.Drawing;

namespace SteamApp.WebAPI.Common;

public static class StaticCollections
{

    // TODO Turn Into Entitity
    public static readonly Paint[] GoodPaintsColorCollection =
[
   new Paint() { Name = "Pink",  Color = Color.FromArgb(192, 255, 105, 180), IsGoodPaint = true},
    new Paint() { Name ="Green", Color = Color.FromArgb(192,  50, 205,  50), IsGoodPaint = true },
   new Paint()  { Name ="Black", Color = Color.FromArgb(192,  20,  20,  20), IsGoodPaint = true},
        new Paint()  { Name ="White", Color = Color.FromArgb(192,  230,  230,  230), IsGoodPaint = true},
        new Paint()  { Name ="Bright Team Color", Color = Color.FromArgb(192,  184,  56,  59), IsGoodPaint = true},
         new Paint()  { Name ="Yellow", Color = Color.FromArgb(192,  231,  181,  59)},
        new Paint()  { Name ="Mint", Color = Color.FromArgb(192,  188,  221,  179)},
        new Paint()  { Name ="Orange", Color = Color.FromArgb(192,  207,  115,  54)},
        new Paint()  { Name ="Purple", Color = Color.FromArgb(192,  125,  64,  113)}
];

    public static readonly string[] GoodPaints =
    [
        "The Bitter Taste of Defeat and Lime", // Lime Green                                                          
        "An Extraordinary Abundance of Tinge", // White
        "A Distinctive Lack of Hue",           // Black
        "Pink as Hell",                        // Pink
        "Team Spirit",                         // Team Color
    ];

    public static readonly Dictionary<string, short> Sheens = new()
    {
        { "Hot Rod", 1 },
        { "Villainous Violet", 2 },
        { "Team Shine", 3 },
        { "Mean Green", 4 },
        { "Agonizing Emerald", 5 },
        { "Mandarin", 6 },
        { "Deadly Daffodil", 7 }
    };

    public static readonly Dictionary<string, short> Colors = new()
    {
        { "Lime", 1 },
        { "White", 2 },
        { "Team Color", 3 },
        { "Black", 4 },
        { "Pink", 5 },
        { "Yellow", 6 },
        { "Mint", 7 }
    };
}
