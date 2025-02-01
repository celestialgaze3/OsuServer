namespace OsuServer.External.Database
{
    public abstract class DbRow
    {
        /// <returns>The arguments used when inserting this row into the database</returns>
        public abstract Dictionary<string, object?> GetInsertionArguments();
    }
}
