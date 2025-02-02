using MySqlConnector;
using OsuServer.External.Database.Tables;

namespace OsuServer.External.Database
{
    public class OsuServerDb(MySqlConnection connection) : DbInstance(connection)
    {
        public DbAccountTable Account { get; set; } = new(connection);
        public DbProfileStatsTable ProfileStats { get; set; } = new(connection);
        public DbScoreTable Score { get; set; } = new(connection);

        public override DbTable[] GetTables()
        {
            return [Account, ProfileStats, Score];
        }
    }
}
