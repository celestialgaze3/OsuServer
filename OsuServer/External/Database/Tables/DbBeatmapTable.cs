using MySqlConnector;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;

namespace OsuServer.External.Database.Tables
{
    public class DbBeatmapTable(DbInstance database) : DbTable<DbBeatmap>(
        database,
        "Beatmap",
        @"id INT UNSIGNED NOT NULL,
        beatmapset_id INT UNSIGNED NOT NULL,
        difficulty_rating FLOAT NOT NULL,
        mode TINYINT UNSIGNED NOT NULL DEFAULT 0,
        ranked_status TINYINT UNSIGNED NOT NULL DEFAULT 0,
        total_length INT NOT NULL DEFAULT 0,
        user_id INT NOT NULL DEFAULT 0,
        version VARCHAR(80) NOT NULL,
        checksum CHAR(32),
        max_combo MEDIUMINT UNSIGNED,
        overall_difficulty FLOAT NOT NULL DEFAULT 0,
        approach_rate FLOAT NOT NULL DEFAULT 0,
        bpm DOUBLE,
        is_convert BOOLEAN NOT NULL DEFAULT 0,
        circle_count MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        slider_count MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        spinner_count MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        circle_size FLOAT NOT NULL DEFAULT 0,
        deleted_at DATETIME,
        hp_drain FLOAT NOT NULL DEFAULT 0,
        hit_length INT NOT NULL DEFAULT 0,
        is_scoreable BOOLEAN NOT NULL DEFAULT 0,
        last_updated DATETIME NOT NULL,
        passcount INT NOT NULL DEFAULT 0,
        playcount INT NOT NULL DEFAULT 0,

        PRIMARY KEY(id)",
        "id")
    {

        protected override DbBeatmap InterpretLatestRecord(MySqlDataReader reader)
        {
            int id = (int)reader.GetUInt32(0);

            return new DbBeatmap(
                new BeatmapExtended(
                    id,
                    (int)reader.GetUInt32(1),
                    reader.GetFloat(2),
                    Ruleset.FromInt(reader.GetByte(3)),
                    RankStatus.FromInt(reader.GetByte(4)),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetString(7),
                    null,
                    reader.IsDBNull(8) ? null : reader.GetString(8),
                    null,
                    null,
                    reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    reader.GetFloat(10),
                    reader.GetFloat(11),
                    reader.IsDBNull(12) ? null : reader.GetFloat(12),
                    reader.GetBoolean(13),
                    reader.GetInt32(14),
                    reader.GetInt32(15),
                    reader.GetInt32(16),
                    reader.GetFloat(17),
                    reader.IsDBNull(18) ? null : reader.GetDateTime(18),
                    reader.GetFloat(19),
                    reader.GetInt32(20),
                    reader.GetBoolean(21),
                    reader.GetDateTime(22),
                    reader.GetInt32(23),
                    reader.GetInt32(24),
                    $"https://osu.ppy.sh/b/{id}"
                )
            );
        }
    }
}
