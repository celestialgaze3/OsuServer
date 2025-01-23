using OsuServer.State;
using OsuServer.Util;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

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

        public float CalculateAccuracy()
        {
            return (float) (Perfects * 300 + Goods * 100 + Bads * 50) / ((Perfects + Goods + Bads + Misses) * 300);
        }

        public string CalculateChecksum(string beatmapMD5, string playerName, string osuVersion, string clientTime, string clientHash, string storyboardChecksum)
        {
            // Not sure why C# decides to put a billion null bytes at the end of this string. Thanks for the debugging nightmare
            string prehash = $"chickenmcnuggets{Perfects + Goods}o15{Bads}{Gekis}smustard{Katus}{Misses}uu{beatmapMD5}" +
                $"{MaxCombo}{PerfectCombo}{playerName}{TotalScore}{Enum.GetName<Grade>(Grade)}{Mods.IntValue}Q{Passed}" +
                $"{(int)GameMode.WithoutMods()}{osuVersion}{clientTime}{clientHash}{storyboardChecksum}";

            prehash = prehash.Trim('\0'); // Trim trailing null bytes

            Console.WriteLine($"Prehash: [{prehash}]");
            Console.WriteLine($"BYTES: [{Convert.ToHexStringLower(Encoding.Unicode.GetBytes(prehash))}]");

            return HashUtil.MD5HashAsUTF8(prehash);
        }
    }
}
