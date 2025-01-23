namespace OsuServer.Objects
{
    public enum GameMode
    {
        Standard = 0,
        Taiko = 1,
        Catch = 2,
        Mania = 3,

        StandardRelax = 4,
        TaikoRelax = 5,
        CatchRelax = 6,
        ManiaRelax = 7,

        StandardAutopilot = 8,
        TaikoAutopilot = 9,
        CatchAutopilot = 10,
        ManiaAutopilot = 11
    }

    static class GameModeExtensions {
    
        public static GameMode WithoutMods(this GameMode mode) {
            return (GameMode)((int)mode % 4); 
        }

    }
}
