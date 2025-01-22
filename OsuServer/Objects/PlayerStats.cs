using System.Runtime.InteropServices;

namespace OsuServer.Objects
{
    public class PlayerStats
    {

        public long TotalScore = 0L;
        public long RankedScore = 0L;
        public float Accuracy = 0f;
        public int Playcount = 0;
        public int Rank = 0;
        public int PP = 0;

        public PlayerStats() { }
        public PlayerStats(long totalScore, long rankedScore, float accuracy, int playcount, int rank, int pp)
        {
            TotalScore = totalScore;
            RankedScore = rankedScore;
            Accuracy = accuracy;
            Playcount = playcount;
            Rank = rank;
            PP = pp;
        }

        public PlayerStats Clone()
        {
            return new PlayerStats(TotalScore, RankedScore, Accuracy, Playcount, Rank, PP);
        }
    }
}
