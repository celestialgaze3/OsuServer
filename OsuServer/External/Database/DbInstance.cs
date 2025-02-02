using MySqlConnector;

namespace OsuServer.External.Database
{
    public abstract class DbInstance
    {
        public MySqlConnection MySqlConnection { get; set; }

        public DbInstance(MySqlConnection connection) 
        {
            MySqlConnection = connection;
        }

        /// <summary>
        /// Initializes the tables in this database instance.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeTables()
        {
            foreach (var table in GetTables())
            {
                await table.CreateTableAsync();
            }
        }

        /// <returns>The tables in the order they should be created</returns>
        public abstract DbTable[] GetTables();
    }
}
