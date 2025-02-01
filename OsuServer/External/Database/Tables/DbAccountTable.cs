using MySqlConnector;
using OsuServer.External.Database.Rows;
using OsuServer.State;
using System.ComponentModel;
using System.Data.Common;

namespace OsuServer.External.Database.Tables
{
    public class DbAccountTable(MySqlConnection connection) : DbTable<DbAccount, int>(connection,
            "Account",
            @"id INT UNSIGNED AUTO_INCREMENT,
            username VARCHAR(15) NOT NULL UNIQUE,
            email VARCHAR(50) NOT NULL UNIQUE,
            password VARCHAR(255) NOT NULL,

            PRIMARY KEY(id)",
            "id")
    {
        public override async Task<int> CreateTableAsync()
        {
            await base.CreateTableAsync();

            // Start auto-incrementing account IDs from 3, to avoid "Do you really want to ask peppy?" when attempting to message ID 2
            var command = new MySqlCommand($"ALTER TABLE {Name} AUTO_INCREMENT=3;", _connection);
            return await command.ExecuteNonQueryAsync();
        }

        protected override DbAccount InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbAccount((int)reader.GetUInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync();
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
