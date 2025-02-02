using MySqlConnector;
using System.Xml.Linq;

namespace OsuServer.External.Database.Rows
{
    public class DbProfileStats : DbRow
    {
        public DbColumn<int> AccountId { get; }
        public DbColumn<long> TotalScore { get; }
        public DbColumn<long> RankedScore { get; }
        public DbColumn<double> Accuracy { get; }
        public DbColumn<int> Playcount { get; }
        public DbColumn<int> Rank { get; }
        public DbColumn<double> PP { get; }
        public DbColumn<int> MaxCombo { get; }

        public DbProfileStats(int id, long totalScore, long rankedScore, double accuracy, int playcount, int rank, double pp, int maxCombo)
        {
            AccountId = new("account_id", id);
            TotalScore = new("total_score", totalScore);
            RankedScore = new("ranked_score", rankedScore);
            Accuracy = new("accuracy", accuracy);
            Playcount = new("playcount", playcount);
            Rank = new("rank", rank);
            PP = new("pp", pp);
            MaxCombo = new("max_combo", maxCombo);
        }

        public override DbColumn[] GetColumns()
        {
            return [AccountId, TotalScore, RankedScore, Accuracy, Playcount, Rank, PP, MaxCombo];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [AccountId];
        }
    }
}
