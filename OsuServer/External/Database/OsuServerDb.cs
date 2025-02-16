using MySqlConnector;
using OsuServer.External.Database.Tables;

namespace OsuServer.External.Database
{
    public class OsuServerDb : DbInstance
    {
        private static bool _tablesInitialized = false;

        public DbAccountTable Account { get; set; }
        public DbProfileStatsTable ProfileStats { get; set; }
        public DbScoreTable Score { get; set; }
        public DbBeatmapTable Beatmap { get; set; }
        public DbFriendTable Friend { get; set; }

        public OsuServerDb(MySqlConnection connection) : base(connection)
        {
            Account = new(this);
            ProfileStats = new(this);
            Score = new(this);
            Beatmap = new(this);
            Friend = new(this);
        }

        public override DbTable[] GetTables()
        {
            return [Account, ProfileStats, Score, Beatmap, Friend];
        }

        public static async Task<OsuServerDb> GetNewConnection()
        {
            var connection = new MySqlConnection($"Server={ServerConfiguration.DatabaseServerIP};" +
                                                 $"User ID={ServerConfiguration.DatabaseUsername};" +
                                                 $"Password={ServerConfiguration.DatabasePassword};" +
                                                 $"Database={ServerConfiguration.DatabaseName};" +
                                                 $"Allow User Variables=True");
            await connection.OpenAsync();

            OsuServerDb database = new(connection);
            if (!_tablesInitialized)
            {
                Console.WriteLine("Initializing tables...");
                await database.InitializeTables();
                Console.WriteLine("Complete!");

                _tablesInitialized = true;
            }
            return database;
        }
    }
}
