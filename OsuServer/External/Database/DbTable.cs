﻿using MySqlConnector;

namespace OsuServer.External.Database
{
    public abstract class DbTable
    {
        protected string _schema;
        protected DbInstance _database;
        protected string _insertionReturnColumns;
        public string Name { get; private set; }

        public DbTable(DbInstance database, string name, string schema, string insertionReturnColumns = "")
        {
            _schema = schema;
            _database = database;
            _insertionReturnColumns = insertionReturnColumns;
            Name = name;
        }

        /// <summary>
        /// Makes a query to information_schema.tables to see if this table exists
        /// </summary>
        /// <returns>Whether or not the table exists</returns>
        public async Task<bool> CheckExistsAsync()
        {
            await _database.EnsureConnectionOpen();

            using var command = new MySqlCommand("SELECT count(*) " +
                "FROM information_schema.tables " +
                $"WHERE table_schema = '{ServerConfiguration.DatabaseName}' " +
                $"AND table_name = '{Name}'", _database.MySqlConnection);

            LogSqlCommand(command);
            long? count = (long?)await command.ExecuteScalarAsync();
            bool tableExisted = count != null && count > 0;

            return tableExisted;
        }

        /// <summary>
        /// Creates the table if it does not exist
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public virtual async Task<int> CreateTableAsync()
        {
            await _database.EnsureConnectionOpen();

            using var command = new MySqlCommand($"CREATE TABLE IF NOT EXISTS {Name} ({_schema});", _database.MySqlConnection);
            LogSqlCommand(command);
            return await command.ExecuteNonQueryAsync();
        }

        protected void LogSqlCommand(MySqlCommand command)
        {
            Console.WriteLine($"Now executing SQL command:\n{command.CommandText}");
        }
    }

    public abstract class DbTable<T> : DbTable where T : DbRow
    {
        public DbTable(DbInstance database, string name, string schema, string insertionReturnColumns = "")
            : base(database, name, schema, insertionReturnColumns)
        { }

        /// <summary>
        /// Gets one <typeparamref name="T"/> from this table
        /// </summary>
        /// <param name="clauses">The clauses to add to the select statement</param>
        /// <returns>The first <typeparamref name="T"/> returned by the database, or null if none was found</returns>
        public async Task<T?> FetchOneAsync(params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            // TODO: fetch only certain columns
            using var command = new MySqlCommand($"SELECT * FROM {Name} " +
                $"{string.Join(" ", clauses.Select(clause => clause.PrepareString()))} LIMIT 1", _database.MySqlConnection);
            
            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var clause in clauses)
            {
                clause.AddParameters(command);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            await using(MySqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return InterpretLatestRecord(reader);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a list of <typeparamref name="T"/> from this table
        /// </summary>
        /// <param name="clauses">The clauses to add to the select statement</param>
        /// <returns>A list of all matching <typeparamref name="T"/></returns>
        public async Task<List<T>> FetchManyAsync(params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            List<T> rows = new();
            // TODO: fetch only certain columns
            using var command = new MySqlCommand($"SELECT * FROM {Name} " +
                $"{string.Join(" ", clauses.Select(clause => clause.PrepareString()))}", _database.MySqlConnection);
            
            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var clause in clauses)
            {
                clause.AddParameters(command);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            await using(MySqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    rows.Add(InterpretLatestRecord(reader));
                }
            }

            return rows;
        }

        /// <summary>
        /// Gets the sum of all values in a column of this table
        /// </summary>
        /// <param name="clauses">The clauses to add to the select statement</param>
        /// <returns>A list of all matching <typeparamref name="T"/></returns>
        public async Task<V?> SumColumn<V>(string columnName, params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            using var command = new MySqlCommand($"SELECT SUM({columnName}) FROM {Name} " +
                $"{string.Join(" ", clauses.Select(clause => clause.PrepareString()))}", _database.MySqlConnection);

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var clause in clauses)
            {
                clause.AddParameters(command);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            object? resultObj = await command.ExecuteScalarAsync();
            if (resultObj is DBNull) return default;
            V? result = (V?)resultObj;
            return result;
        }

        /// <summary>
        /// Gets the max of all values in a column of this table
        /// </summary>
        /// <param name="clauses">The clauses to add to the select statement</param>
        /// <returns>A list of all matching <typeparamref name="T"/></returns>
        public async Task<V?> MaxColumn<V>(string columnName, params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            using var command = new MySqlCommand($"SELECT MAX({columnName}) FROM {Name} " +
                $"{string.Join(" ", clauses.Select(clause => clause.PrepareString()))}", _database.MySqlConnection);

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var clause in clauses)
            {
                clause.AddParameters(command);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            object? resultObj = await command.ExecuteScalarAsync();
            if (resultObj is DBNull) return default;
            V? result = (V?)resultObj;
            return result;
        }


        /// <summary>
        /// Gets the ordered index of a row when sorted by one of its columns
        /// </summary>
        /// <param name="row">The row to get the ordered index of</param>
        /// <param name="orderByColumn">The column name to order by</param>
        /// <returns>The index of this column</returns>
        public async Task<int> GetRankAsync(T row, string orderByColumn, string innerWhereClause = "", string innerJoinClause = "")
        {
            await _database.EnsureConnectionOpen();
            DbColumn[] identifyingColumns = row.GetIdentifyingColumns();

            Dictionary<string, object?> values = new();
            foreach (var column in identifyingColumns)
            {
                values.Add(column.Name, column.Object);
            }

            DbClause whereClause = new DbClause(
                "WHERE",
                string.Join(" AND ", identifyingColumns.Select(entry => $"{entry.Name} = {entry.Object}")),
                values
            );

            string identifyingColumnsSelect = string.Join(
                ", ",
                identifyingColumns.Select(entry => $"{Name}.{entry.Name}")
            );

            // Can't figure out how to parameterize this and have user variables at the same time. Not a big issue though
            // TODO: scuffed as hell, but will be fixed if i figure out above
            using var command = new MySqlCommand($"SELECT ordered.position " +
                $"FROM (SELECT {identifyingColumnsSelect}, {Name}.{orderByColumn}, (@pos := @pos + 1) AS position " +
                $"FROM {Name}" + (string.IsNullOrEmpty(innerJoinClause) ? "" : $" {innerJoinClause}") + 
                " JOIN (SELECT @pos := 0) AS x " +
                (innerWhereClause != string.Empty ? $"WHERE {innerWhereClause} " : "") +
                $"ORDER BY {Name}.{orderByColumn} DESC) AS ordered " +
                $"{whereClause.PrepareString()}", _database.MySqlConnection);

            LogSqlCommand(command);

            object? rank = (object?)await command.ExecuteScalarAsync();
            return Convert.ToInt32(rank);
        }

        /// <summary>
        /// Gets the command that would insert the given data into this table.
        /// </summary>
        /// <param name="insertion">The <typeparamref name="T"/> to insert</param>
        /// <returns>The command (unexecuted)</returns>
        protected async Task<MySqlCommand> GetInsertCommand(T insertion)
        {
            await _database.EnsureConnectionOpen();

            Dictionary<string, object?> insertionArguments = insertion.GetInsertionArguments();
            string columnNames = string.Join(", ", insertionArguments.Select(entry => entry.Key));
            string valueNames = string.Join(", ", insertionArguments.Select(entry => $"@{entry.Key}"));

            var command = new MySqlCommand($"INSERT INTO {Name}({columnNames}) VALUES ({valueNames})" +
                (_insertionReturnColumns.Length > 0 ? $" RETURNING {_insertionReturnColumns}" : ""), _database.MySqlConnection);

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var entry in insertionArguments)
            {
                command.Parameters.AddWithValue($"@{entry.Key}", entry.Value);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();
            
            return command;
        }

        /// <summary>
        /// Inserts a row into this table
        /// </summary>
        /// <param name="insertion">The <typeparamref name="T"/> to insert</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<int> InsertAsync(T insertion)
        {
            using MySqlCommand command = await GetInsertCommand(insertion);
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Deletes a row from this table
        /// </summary>
        /// <param name="insertion">The <typeparamref name="T"/> to delete</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DeleteOneAsync(T deletion)
        {
            await _database.EnsureConnectionOpen();

            DbColumn[] identifyingColumns = deletion.GetIdentifyingColumns();

            Dictionary<string, object?> values = new();
            foreach (var column in identifyingColumns)
            {
                values.Add(column.Name, column.Object);
            }

            DbClause whereClause = new(
                "WHERE",
                string.Join(" AND ", identifyingColumns.Select(entry => $"{entry.Name} = @{entry.Name}")),
                values
            );

            using var command = new MySqlCommand($"DELETE FROM {Name} {whereClause.PrepareString()}", _database.MySqlConnection);

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            whereClause.AddParameters(command);

            LogSqlCommand(command);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<long> GetRowCountAsync(params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            using var command = new MySqlCommand($"SELECT COUNT(*) FROM {Name} " +
                $"{string.Join(" ", clauses.Select(clause => clause.PrepareString()))}", _database.MySqlConnection);

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            foreach (var clause in clauses)
            {
                clause.AddParameters(command);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            long? count = (long?)await command.ExecuteScalarAsync();
            if (count == null) return 0;
            return (long)count;
        }

        /// <summary>
        /// Updates an existing row in this table
        /// </summary>
        /// <param name="insertion">Data from a <typeparamref name="T"/> to use for the update</param>
        /// <param name="clauses">The clauses to add to the update statement</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UpdateAsync(T insertion, params DbClause[] clauses)
        {
            await _database.EnsureConnectionOpen();

            Dictionary<string, object?> updateArguments = insertion.GetUpdateArguments();
            if (updateArguments.Count == 0) return; // Nothing to update
            DbClause setClause = new(
                "SET",
                string.Join(", ", updateArguments.Select(entry => $"{entry.Key} = @{entry.Key}")),
                updateArguments
            );

            // Prepend the set clause
            DbClause[] clausesWithSet = new DbClause[clauses.Length + 1];
            clausesWithSet[0] = setClause;
            Array.Copy(clauses, 0, clausesWithSet, 1, clauses.Length);

            using var command = new MySqlCommand(
                $"UPDATE {Name} {string.Join(" ", clausesWithSet.Select(clause => clause.PrepareString()))}",
                _database.MySqlConnection
            );

            if (_database.Transaction != null)
                command.Transaction = _database.Transaction;

            // Combine parameters 
            Dictionary<string, object?> combinedParams = [];
            foreach (var entry in updateArguments)
            {
                combinedParams[$"@{entry.Key}"] = entry.Value;
            }
            foreach (var clause in clausesWithSet)
            {
                clause.AddParameters(combinedParams);
            }

            foreach(var parameter in combinedParams)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            LogSqlCommand(command);
            await command.PrepareAsync();

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Updates one existing row in this table
        /// </summary>
        /// <param name="insertion">The specific <typeparamref name="T"/> to update</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UpdateOneAsync(T insertion)
        {
            DbColumn[] identifyingColumns = insertion.GetIdentifyingColumns();

            Dictionary<string, object?> values = new();
            foreach (var column in identifyingColumns)
            {
                values.Add(column.Name, column.Object);
            }

            DbClause whereClause = new DbClause(
                "WHERE",
                string.Join(" AND ", identifyingColumns.Select(entry => $"{entry.Name} = @{entry.Name}")),
                values
            );

            await UpdateAsync(insertion, whereClause);
        }

        /// <summary>
        /// Interprets the latest record from the given data reader
        /// </summary>
        /// <param name="reader">The data reader</param>
        /// <returns>The <typeparamref name="T"/> representing this record</returns>
        protected abstract T InterpretLatestRecord(MySqlDataReader reader);
    }

    public abstract class DbTable<T, U> : DbTable<T> where T : DbRow
    {
        protected DbTable(DbInstance database, string name, string schema, string insertionReturnColumns = "") 
            : base(database, name, schema, insertionReturnColumns) {}

        /// <summary>
        /// Inserts a row into this table
        /// </summary>
        /// <param name="insertion">The <typeparamref name="T"/> to insert</param>
        /// <param name="bodgyReconnect">See <see cref="DbInstance.CleanConnection"/>. Set to false to avoid
        /// disposing the connection before a transaction completes. The bad news is that we can only ever
        /// insert one row in a single transaction. Hopefully that never becomes an issue or I manage to
        /// fix this problem.</param>
        /// <returns>Information about the insertion of type <typeparamref name="U"/></returns>
        public async Task<U> InsertAsync(T insertion, bool bodgyReconnect = true)
        {
            using MySqlCommand command = await GetInsertCommand(insertion);

            MySqlDataReader reader = await command.ExecuteReaderAsync();
            U insertionResult = await ReadInsertion(reader);
            await reader.DisposeAsync();

            // *sigh*
            _database.IsDirty = true;
            if (bodgyReconnect)
                await _database.CleanConnection();

            return insertionResult;
        }

        /// <summary>
        /// Reads from the data reader returned by an insert operation, and updates this state of this object
        /// </summary>
        /// <param name="reader">The data reader</param>
        /// <returns>The <typeparamref name="U"/> an insert operation should return</returns>
        protected abstract Task<U> ReadInsertion(MySqlDataReader reader);
    }
}
