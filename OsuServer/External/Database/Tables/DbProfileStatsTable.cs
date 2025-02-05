using MySqlConnector;
using OsuServer.External.Database.Rows;
using OsuServer.Objects;

namespace OsuServer.External.Database.Tables
{
    public class DbProfileStatsTable(DbInstance database) : DbTable<DbProfileStats, int>(
        database,
        "ProfileStats",
        @"account_id INT UNSIGNED NOT NULL,
        gamemode TINYINT UNSIGNED NOT NULL DEFAULT 0,
        total_score BIGINT NOT NULL DEFAULT 0,
        ranked_score BIGINT NOT NULL DEFAULT 0,
        accuracy DOUBLE NOT NULL DEFAULT 0,
        playcount INT NOT NULL DEFAULT 0,
        rank INT NOT NULL DEFAULT 0,
        pp DOUBLE NOT NULL DEFAULT 0,
        max_combo INT NOT NULL DEFAULT 0,

        PRIMARY KEY(account_id, gamemode),
        CONSTRAINT FK_profile_stats_account FOREIGN KEY (account_id) REFERENCES Account(id)",
        "account_id")
    {

        protected override DbProfileStats InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbProfileStats(
                (int)reader.GetUInt32(0),
                (GameMode)reader.GetInt32(1),
                reader.GetInt64(2), 
                reader.GetInt64(3),
                reader.GetDouble(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetDouble(7),
                reader.GetInt32(8)
            );
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync();
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
