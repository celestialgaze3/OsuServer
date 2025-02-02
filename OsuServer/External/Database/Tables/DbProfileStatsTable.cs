using MySqlConnector;
using OsuServer.External.Database.Rows;

namespace OsuServer.External.Database.Tables
{
    public class DbProfileStatsTable(DbInstance database) : DbTable<DbProfileStats, int>(
        database,
        "ProfileStats",
        @"account_id INT UNSIGNED NOT NULL,
        total_score BIGINT NOT NULL DEFAULT 0,
        ranked_score BIGINT NOT NULL DEFAULT 0,
        accuracy DOUBLE NOT NULL DEFAULT 0,
        playcount INT NOT NULL DEFAULT 0,
        rank INT NOT NULL DEFAULT 0,
        pp DOUBLE NOT NULL DEFAULT 0,
        max_combo INT NOT NULL DEFAULT 0,

        PRIMARY KEY(account_id),
        CONSTRAINT FK_profile_stats_account FOREIGN KEY (account_id) REFERENCES Account(id)",
        "account_id")
    {

        protected override DbProfileStats InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbProfileStats(
                (int)reader.GetUInt32(0),
                reader.GetInt64(1), 
                reader.GetInt64(2),
                reader.GetDouble(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetDouble(6),
                reader.GetInt32(7)
            );
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync();
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
