namespace OsuServer.Objects
{
    public class Score
    {
        public int Perfects { get; private set; }
        public int Goods { get; private set; }
        public int Bads { get; private set; }
        public int Gekis { get; private set; }
        public int Katus { get; private set; }
        public int Misses { get; private set; }
        public int TotalScore { get; private set; }
        public int MaxCombo { get; private set; }
        public bool PerfectCombo { get; private set; }
        public Grade Grade { get; private set; }
        public Mods Mods { get; private set; }
        public bool Passed { get; private set; }
        public GameMode GameMode { get; private set; }

        // TODO: Possibly add Beatmap and Player reference?
        // TODO: Add checksum calculator

        public Score(int perfects, int goods, int bads, int gekis, int katus, int misses, int totalScore, int maxCombo, bool perfectCombo,
            Grade grade, Mods mods, bool passed, GameMode gameMode)
        {
            Perfects = perfects;
            Goods = goods;
            Bads = bads;
            Gekis = gekis;
            Katus = katus;
            Misses = misses;
            TotalScore = totalScore;
            MaxCombo = maxCombo;
            PerfectCombo = perfectCombo;
            Grade = grade;
            Mods = mods;
            Passed = passed;
            GameMode = gameMode;
        }
    }
}
