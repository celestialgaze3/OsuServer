using MySqlConnector;
using OsuServer.External.Database.Tables;

namespace OsuServer.External.Database
{
    public class OsuServerDb : DbInstance
    {
        public DbAccountTable Account { get; set; }
        public DbProfileStatsTable ProfileStats { get; set; }
        public DbScoreTable Score { get; set; }
        public DbBeatmapTable Beatmap { get; set; }

        public OsuServerDb(MySqlConnection connection) : base(connection)
        {
            Account = new(this);
            ProfileStats = new(this);
            Score = new(this);
            Beatmap = new(this);
        }

        public override DbTable[] GetTables()
        {
            return [Account, ProfileStats, Score, Beatmap];
        }
    }
}
