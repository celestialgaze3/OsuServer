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
        StandardAutopilot = 7
    }

    static class GameModeHelper {
        public static GameMode[] GetAll()
        {
            return Enum.GetValues<GameMode>();
        }

        public static GameMode[] GetMain()
        {
            return [GameMode.Standard, GameMode.Taiko, GameMode.Catch, GameMode.Mania];
        }

        public static GameMode WithoutMods(this GameMode mode) {
            return (GameMode)((int)mode % 4); 
        }

        public static GameMode WithMods(this GameMode mode, Mods mods)
        {
            GameMode withoutMods = mode.WithoutMods();
            if (mods.Has(Mod.Relax) && withoutMods != GameMode.Mania)
                return withoutMods + 4;
            if (mods.Has(Mod.Autopilot) && withoutMods == GameMode.Standard)
                return GameMode.StandardAutopilot;
            return mode;
        }

    }
}
