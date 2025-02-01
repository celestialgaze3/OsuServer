using MySqlConnector;

namespace OsuServer.External.Database
{
    public class DbClause
    {
        private string _name;
        private string _format;
        private Dictionary<string, object?> _values;

        public string Name { 
            get { return _name; }
        }

        public Dictionary<string, object?> Values
        {
            get { return _values; }
        }

        public DbClause(string clauseName, string clauseFormat)
        {
            _name = clauseName;
            _format = clauseFormat;
            _values = new();
        }

        public DbClause(string clauseName, string clauseFormat, Dictionary<string, object?> values)
        {
            _name = clauseName;
            _format = clauseFormat;
            _values = values;
        }

        public string PrepareString()
        {
            return $"{_name} {_format}";
        }

        public void AddParameters(MySqlCommand command)
        {
            foreach (var parameter in _values) {
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
            }
        }

        public void AddParameters(Dictionary<string, object?> values)
        {
            foreach (var parameter in _values)
            {
                values[$"@{parameter.Key}"] = parameter.Value;
            }
        }

    }
}
