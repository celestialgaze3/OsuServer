namespace OsuServer.Objects
{
    public class Mods
    {
        public int IntValue { get; set; } = 0;

        public Mods() { }

        public Mods(Mod mod)
        {
            IntValue = (int)mod;
        }

        public Mods(int mods)
        {
            IntValue = mods;
        }

        public void Add(Mod mod)
        {
            IntValue |= (int)mod;
        }

        public void Remove(Mod mod)
        {
            IntValue &= ~(int)mod;
        }

        public bool Has(Mod mod)
        {
            return (IntValue & (int)mod) > 0;
        }

        public List<Mod> GetAll()
        {
            var list = new List<Mod>();
            foreach (Mod mod in Enum.GetValues(typeof(Mod)))
            {
                if (Has(mod))
                {
                    list.Add(mod);
                }
            }

            return list;
        }
    }

    public enum Mod
    {
        None = 0,
        NoFail = 1 << 0,
        Easy = 1 << 1,
        Touchscreen = 1 << 2,
        Hidden = 1 << 3,
        HardRock = 1 << 4,
        SuddenDeath = 1 << 5,
        DoubleTime = 1 << 6,
        Relax = 1 << 7,
        HalfTime = 1 << 8,
        Nightcore = 1 << 9,
        Flashlight = 1 << 10,
        Autoplay = 1 << 11,
        SpunOut = 1 << 12,
        Autopilot = 1 << 13,
        Perfect = 1 << 14,
        Key4 = 1 << 15,
        Key5 = 1 << 16,
        Key6 = 1 << 17,
        Key7 = 1 << 18,
        Key8 = 1 << 19,
        FadeIn = 1 << 20,
        Random = 1 << 21,
        Cinema = 1 << 22,
        TargetPractice = 1 << 23,
        Key9 = 1 << 24,
        KeyCoop = 1 << 25,
        Key1 = 1 << 26,
        Key3 = 1 << 27,
        Key2 = 1 << 28,
        ScoreV2 = 1 << 29,
        Mirror = 1 << 30
    }
}
