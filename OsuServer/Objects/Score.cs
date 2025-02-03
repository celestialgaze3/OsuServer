using OsuServer.External.Database.Rows;
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

        public Player Player { get; private set; }
        public BanchoBeatmap Beatmap { get; private set; }
        public long Timestamp { get; internal set; }

        public Score(int perfects, int goods, int bads, int gekis, int katus, int misses, int totalScore, int maxCombo, bool perfectCombo,
            Grade grade, Mods mods, bool passed, GameMode gameMode, Player player, BanchoBeatmap beatmap, long timestamp)
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
            Player = player;
            Beatmap = beatmap;
            Timestamp = timestamp;
        }

        public static async Task<Score> Get(Bancho bancho, DbScore dbScore)
        {
            GameMode gameMode = (GameMode)dbScore.GameMode.Value;
            Mods mods = new Mods(dbScore.Mods.Value);
            int perfects = dbScore.Perfects.Value;
            int goods = dbScore.Goods.Value;
            int bads = dbScore.Bads.Value;
            int misses = dbScore.Misses.Value;
            bool isPass = dbScore.IsPass.Value;

            return new Score(
                perfects,
                goods,
                bads,
                misses,
                dbScore.Katus.Value,
                dbScore.Misses.Value,
                dbScore.TotalScore.Value,
                dbScore.MaxCombo.Value,
                dbScore.IsPerfectCombo.Value,
                CalculateGrade(gameMode, mods, perfects, goods, bads, misses, isPass),
                mods,
                isPass,
                gameMode,
                bancho.GetPlayer((int)dbScore.AccountId.Value),
                await bancho.GetBeatmap((int)dbScore.BeatmapId.Value),
                dbScore.SubmittedTime.Value
            );
        }

        public double CalculateAccuracy()
        {
            return CalculateAccuracy(Perfects, Goods, Bads, Misses);
        }

        public static double CalculateAccuracy(int perfects, int goods, int bads, int misses)
        {
            return (double)(perfects * 300 + goods * 100 + bads * 50) / ((perfects + goods + bads + misses) * 300);
        }

        public string CalculateChecksum(string beatmapMD5, string playerName, string osuVersion, string clientTime, string clientHash, string storyboardChecksum)
        {

            // Not sure why C# decides to put a billion null bytes at the end of strings. Thanks for the debugging nightmare
            /*string prehash = $"chickenmcnuggets{Perfects + Goods}o15{Bads}{Gekis}smustard{Katus}{Misses}uu{beatmapMD5}" +
                $"{MaxCombo}{PerfectCombo}{playerName}{TotalScore}{Enum.GetName<Grade>(Grade)}{Mods.IntValue}Q{Passed}" +
                $"{(int)GameMode.WithoutMods()}{osuVersion}{clientTime}{clientHash}{storyboardChecksum}";*/

            // We're trimming all the strings that go in and out of this thing because C# loves throwing around null bytes and not cleaning up.
            StringBuilder sb = new StringBuilder();
            sb.Append("chickenmcnuggets");
            sb.Append(Perfects + Goods);
            sb.Append("o15");
            sb.Append(Bads);
            sb.Append(Gekis);
            sb.Append("smustard");
            sb.Append(Katus);
            sb.Append(Misses);
            sb.Append("uu");
            sb.Append(beatmapMD5.TrimEnd('\0'));
            sb.Append(MaxCombo);
            sb.Append(PerfectCombo);
            sb.Append(playerName.TrimEnd('\0'));
            sb.Append(TotalScore);
            sb.Append(Enum.GetName<Grade>(Grade));
            sb.Append(Mods.IntValue);
            sb.Append("Q");
            sb.Append(Passed);
            sb.Append((int)GameMode.WithoutMods());
            sb.Append(osuVersion.TrimEnd('\0'));
            sb.Append(clientTime.TrimEnd('\0'));
            sb.Append(clientHash.TrimEnd('\0'));
            sb.Append(storyboardChecksum.TrimEnd('\0'));

            string prehash = sb.ToString().TrimEnd('\0');

            return HashUtil.MD5HashAsUTF8(prehash);
        }

        public static Grade CalculateGrade(GameMode gameMode, Mods mods, int perfects, int goods, int bads, int misses, bool passed)
        {
            if (!passed) 
                return Grade.F;

            if (gameMode.WithoutMods() == GameMode.Standard)
            {
                int totalObjects = perfects + goods + bads + misses;
                double percentPerfect = (double)perfects / totalObjects;

                // SS
                if (perfects == totalObjects)
                {
                    if (mods.Has(Mod.Hidden) || mods.Has(Mod.Flashlight))
                        return Grade.XH;
                    return Grade.X;
                }

                if (percentPerfect >= 0.9d)
                {
                    // S
                    if ((double)bads / totalObjects <= 0.01d && misses == 0)
                    {
                        if (mods.Has(Mod.Hidden) || mods.Has(Mod.Flashlight))
                            return Grade.SH;
                        return Grade.S;
                    }

                    // A
                    return Grade.A;
                }

                if (percentPerfect >= 0.8d)
                {
                    // A
                    if (misses == 0)
                    {
                        return Grade.A;
                    }

                    // B
                    return Grade.B;
                }

                // B
                if (percentPerfect >= 0.7d && misses == 0)
                {
                    return Grade.B;
                }

                // C
                if (percentPerfect >= 0.6d)
                {
                    return Grade.C;
                }

                return Grade.D;

            }

            // TODO: implement grade calculations for other gamemodes
            return Grade.None;
        }
    }
}
