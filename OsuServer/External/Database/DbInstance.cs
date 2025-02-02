using MySqlConnector;
using System.Runtime.CompilerServices;

namespace OsuServer.External.Database
{
    public abstract class DbInstance
    {
        public MySqlTransaction? Transaction;
        public MySqlConnection MySqlConnection { get; set; }

        public DbInstance(MySqlConnection connection) 
        {
            MySqlConnection = connection;
        }

        public async Task EnsureConnectionOpen()
        {
            if (MySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                Console.WriteLine("Disconnected from MySQL database. Reconnecting...");
                await MySqlConnection.OpenAsync();
                Console.WriteLine("Complete!");
            }
        }

        /// <summary>
        /// Starts a transaction 
        /// </summary>
        /// <returns></returns>
        public async Task StartTransaction()
        {
            await EnsureConnectionOpen();

            Transaction = await MySqlConnection.BeginTransactionAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if there is no active transaction to commit.</exception>
        public async Task CommitTransaction()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("There is no active transaction to commit!");
            }

            await Transaction.CommitAsync();
            await Transaction.DisposeAsync();

            Transaction = null;
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
