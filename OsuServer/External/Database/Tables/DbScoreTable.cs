using MySqlConnector;
using OsuServer.External.Database.Rows;

namespace OsuServer.External.Database.Tables
{
    public class DbScoreTable(DbInstance database) : DbTable<DbScore, int>(
        database,
        "Score",
        @"id INT UNSIGNED NOT NULL AUTO_INCREMENT,
        account_id INT UNSIGNED NOT NULL,
        beatmap_id INT UNSIGNED NOT NULL,
        perfects MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        goods MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        bads MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        gekis MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        katus MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        misses MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        total_score INT NOT NULL DEFAULT 0,
        max_combo MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        is_perfect_combo BOOLEAN NOT NULL DEFAULT 0,
        mods INT NOT NULL DEFAULT 0,
        is_pass BOOLEAN NOT NULL DEFAULT 0,
        gamemode TINYINT UNSIGNED NOT NULL DEFAULT 0,
        pp DOUBLE NOT NULL DEFAULT 0,
        is_best_pp BOOLEAN NOT NULL DEFAULT 0,
        is_best_accuracy BOOLEAN NOT NULL DEFAULT 0,
        is_best_combo BOOLEAN NOT NULL DEFAULT 0,
        is_best_score BOOLEAN NOT NULL DEFAULT 0,
        submitted_time BIGINT NOT NULL DEFAULT 0,

        PRIMARY KEY(id),
        CONSTRAINT FK_score_account FOREIGN KEY (account_id) REFERENCES Account(id)",
        "id")
    {

        protected override DbScore InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbScore(
                reader.GetUInt32(0),
                reader.GetUInt32(1),
                reader.GetUInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetInt32(8),
                reader.GetInt32(9),
                reader.GetInt32(10),
                reader.GetBoolean(11),
                reader.GetInt32(12),
                reader.GetBoolean(13),
                reader.GetByte(14),
                reader.GetDouble(15),
                reader.GetBoolean(16),
                reader.GetBoolean(17),
                reader.GetBoolean(18),
                reader.GetBoolean(19),
                reader.GetInt64(20)
            );
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync();
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
