using MySqlConnector;

namespace OsuServer.External.Database
{
    public abstract class DbInstance
    {
        public MySqlTransaction? Transaction;
        public MySqlConnection MySqlConnection { get; set; }

        /// <summary>
        /// See <see cref="CleanConnection"/>
        /// </summary>
        public bool IsDirty { get; set; } = false;

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
        /// There's no real need to do this. It's a very bodgy bug fix. 
        /// An error was encountered when doing multiple inserts with the same connection:
        /// "System.InvalidOperationException: Expected result set to have 0 columns, but it contains 1 columns."
        /// The error would be thrown after attempting a second insert. Not sure what the root of the issue is. 
        /// Also not sure how much time calling this wastes, but it's probably not insignificant. Ideally this 
        /// should never be used.
        /// </summary>
        /// <returns></returns>
        public async Task CleanConnection()
        {
            if (MySqlConnection.State == System.Data.ConnectionState.Open)
            {
                await MySqlConnection.CloseAsync();
                await MySqlConnection.OpenAsync();

                IsDirty = false;
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

            if (IsDirty)
                await CleanConnection();
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
