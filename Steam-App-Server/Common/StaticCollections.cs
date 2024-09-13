namespace SteamAppServer.Common
{
    public static class StaticCollections
    {
        public static readonly string[] GoodPaintsStringValues = new[]
        {
            "Paint Color: The Bitter Taste of Defeat and Lime", // Lime Green                                                          
            "Paint Color: An Extraordinary Abundance of Tinge", // White
            "Paint Color: A Distinctive Lack of Hue",           // Black
            "Paint Color: Pink as Hell",                        // Pink
            "Paint Color: Team Spirit",                         // Team Color
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
    }
}
