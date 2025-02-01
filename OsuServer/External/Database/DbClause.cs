using MySqlConnector;

namespace OsuServer.External.Database
{
    public class DbClause
    {
        private string _clauseName;
        private string _clauseFormat;
        private Dictionary<string, object?> values;

        public DbClause(string clauseName, string clauseFormat)
        {
            _clauseName = clauseName;
            _clauseFormat = clauseFormat;
            this.values = new();
        }

        public DbClause(string clauseName, string clauseFormat, Dictionary<string, object?> values)
        {
            _clauseName = clauseName;
            _clauseFormat = clauseFormat;
            this.values = values;
        }

        public string PrepareString()
        {
            return $"{_clauseName} {_clauseFormat}";
        }

        public void AddParameters(MySqlCommand command)
        {
            foreach (var parameter in this.values) {
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
            }
        }

    }
}
