namespace OsuServer.External.Database
{
    public abstract class DbRow
    {
        /// <returns>The arguments used when inserting this row into the database</returns>
        public Dictionary<string, object?> GetInsertionArguments()
        {
            Dictionary<string, object?> result = new();
            foreach (DbColumn column in GetColumns())
            {
                if (column.CanModify)
                {
                    result[column.Name] = column.Object;
                }
            }

            return result;
        }

        public Dictionary<string, object?> GetUpdateArguments()
        {
            Dictionary<string, object?> result = new();
            foreach (DbColumn column in GetColumns()) 
            {
                if (column.CanModify && column.HasBeenModified)
                {
                    result[column.Name] = column.Object;
                }
            }

            return result;
        }

        /// <returns>The columns this row contains</returns>
        public abstract DbColumn[] GetColumns();

        /// <returns>The columns that would uniquely identify this row (primary key)</returns>
        public abstract DbColumn[] GetIdentifyingColumns();
    }
}
