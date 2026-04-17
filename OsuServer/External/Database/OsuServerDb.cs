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

        public OsuServerDb(MySqlConnection connection, string rawConnectionString) : base(connection, rawConnectionString)
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
            string rawConnectionString = $"Server={ServerConfiguration.DatabaseServerIP};" +
                                         $"User ID={ServerConfiguration.DatabaseUsername};" +
                                         $"Password={ServerConfiguration.DatabasePassword};" +
                                         $"Database={ServerConfiguration.DatabaseName};" +
                                         $"Allow User Variables=True";
            var connection = new MySqlConnection(rawConnectionString);
            await connection.OpenAsync();

            OsuServerDb database = new(connection, rawConnectionString);
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
