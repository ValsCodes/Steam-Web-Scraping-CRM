using SteamApp.Models.ValueObjects;
using System.Drawing;

namespace SteamApp.WebAPI.Common;

public static class StaticCollections
{

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

    public static readonly string[] GoodPaints = new[]
    {
        "The Bitter Taste of Defeat and Lime", // Lime Green                                                          
        "An Extraordinary Abundance of Tinge", // White
        "A Distinctive Lack of Hue",           // Black
        "Pink as Hell",                        // Pink
        "Team Spirit",                         // Team Color
    };

    public static readonly Dictionary<string, short> Sheens = new Dictionary<string, short>
    {
        { "Hot Rod", 1 },
        { "Villainous Violet", 2 },
        { "Team Shine", 3 },
        { "Mean Green", 4 },
        { "Agonazing Emerald", 5 },
        { "Manndarin", 6 },
        { "Deadly Daffodil", 7 }
    };

    public static readonly Dictionary<string, short> Colors = new Dictionary<string, short>
    {
        { "Lime", 1 },
        { "White", 2 },
        { "Team Color", 3 },
        { "Black", 4 },
        { "Pink", 5 },
        { "Yellow", 6 },
        { "Mint", 7 }
    };

    public static string[] WEAPON_NAMES =
        {
        "Degreaser","Backburner",
"Phlogistinator",
"Flame%20Thrower",
"Scattergun",
"Force-A-Nature",
// "Guilotine",
"Rocket%20Launcher",
"Direct%20Hit",
"Black%20Box",
"Minigun",
"Rescue%20Ranger",
"Crusader%27s%20Crossbow",
"Sniper%20Rifle",
"L%27Etranger",
"Shotgun",
"Spy-cicle",
"Tomislav",
"Medi%20Gun",
"Axtinguisher",
"Kukri",
"Powerjack",
"Fists%20of%20Steel",
"Bushwacka",
"Your%20Eternal%20Reward",
"Gloves%20of%20Running%20Urgently",
"Detonator",
"SMG",
"Holiday%20Punch",
"Jag",
"Conniver%27s%20Kunai",
"Escape%20Plan",
"Scorch%20Shot",
"Wrench",
"Disciplinary%20Action",
"Flare%20Gun",
"Eyelander",
"Hitman%27s%20Heatmaker",
"Quick-Fix",
"Knife",
"Market%20Gardener",
"Backburner",
"Bottle",
"Machina",
"Revolver",
"Ambassador",
"Pistol",
"Wrangler",
"Stickybomb%20Launcher",
"Frontier%20Justice",
"Kritzkrieg",
"Huntsman",
"Grenade%20Launcher",
"Cleaner%27s%20Carbine",
"Quickiebomb%20Launcher",
"Ubersaw",
};
}
