using MySqlConnector;
using System.Xml.Linq;

namespace OsuServer.External.Database.Rows
{
    public class DbAccount : DbRow
    {
        public int Id { get; private set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public DbAccount(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }

        public DbAccount(int id, string username, string email, string password)
        {
            Id = id;
            Username = username;
            Email = email;
            Password = password;
        }

        public override Dictionary<string, object?> GetInsertionArguments()
        {
            return new()
            {
                ["id"] = null,
                ["username"] = Username,
                ["email"] = Email,
                ["password"] = Password
            };
        }
    }
}
