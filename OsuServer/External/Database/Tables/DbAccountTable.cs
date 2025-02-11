using MySqlConnector;
using OsuServer.External.Database.Rows;

namespace OsuServer.External.Database.Tables
{
    public class DbAccountTable(DbInstance database) : DbTable<DbAccount, int>(
        database,
        "Account",
        @"id INT UNSIGNED AUTO_INCREMENT,
        username VARCHAR(15) NOT NULL UNIQUE,
        email VARCHAR(50) NOT NULL UNIQUE,
        password VARCHAR(255) NOT NULL,
        registration_time BIGINT NOT NULL,
        last_activity_time BIGINT NOT NULL,
        country_code_num TINYINT UNSIGNED,

        PRIMARY KEY(id)",
        "id")
    {
        public override async Task<int> CreateTableAsync()
        {
            bool existed = await CheckExistsAsync();
            await base.CreateTableAsync();

            // Start auto-incrementing account IDs from 3, to avoid "Do you really want to ask peppy?" when attempting to message ID 2
            if (!existed)
            {
                using var command = new MySqlCommand($"ALTER TABLE {Name} AUTO_INCREMENT=3;", _database.MySqlConnection);
                return await command.ExecuteNonQueryAsync();
            }
            return 0;
        }

        protected override DbAccount InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbAccount(
                (int)reader.GetUInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt64(4),
                reader.GetInt64(5),
                reader.IsDBNull(6) ? null : reader.GetByte(6)
            );
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync().ConfigureAwait(false);
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
