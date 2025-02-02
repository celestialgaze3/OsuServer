using System.Runtime.InteropServices;

namespace OsuServer.Objects
{
    public class ProfileStats
    {

        public long TotalScore = 0L;
        public long RankedScore = 0L;
        public double Accuracy = 0d;
        public int Playcount = 0;
        public int Rank = 0;
        public double PP = 0.0d;
        public int MaxCombo = 0;

        public ProfileStats() { }
        public ProfileStats(long totalScore, long rankedScore, double accuracy, int playcount, int rank, double pp, int maxCombo)
        {
            TotalScore = totalScore;
            RankedScore = rankedScore;
            Accuracy = accuracy;
            Playcount = playcount;
            Rank = rank;
            PP = pp;
            MaxCombo = maxCombo;
        }

        public ProfileStats Clone()
        {
            return new ProfileStats(TotalScore, RankedScore, Accuracy, Playcount, Rank, PP, MaxCombo);
        }
    }
}
