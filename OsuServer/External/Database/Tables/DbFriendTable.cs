using MySqlConnector;
using OsuServer.External.Database.Rows;

namespace OsuServer.External.Database.Tables
{
    public class DbFriendTable(DbInstance database) : DbTable<DbFriend, int>(
        database,
        "Friend",
        @"id INT UNSIGNED NOT NULL,
        friend_id INT UNSIGNED NOT NULL,

        PRIMARY KEY(id, friend_id),
        CONSTRAINT FK_friend_adder FOREIGN KEY (id) REFERENCES Account(id),
        CONSTRAINT FK_friend_addee FOREIGN KEY (friend_id) REFERENCES Account(id)",
        "id")
    {
        protected override DbFriend InterpretLatestRecord(MySqlDataReader reader)
        {
            return new DbFriend(
                (int)reader.GetUInt32(0),
                (int)reader.GetUInt32(1)
            );
        }

        protected override async Task<int> ReadInsertion(MySqlDataReader reader)
        {
            await reader.ReadAsync().ConfigureAwait(false);
            return (int)reader.GetUInt32(0); // Returns id
        }
    }
}
