using MySqlConnector;
using System.Xml.Linq;

namespace OsuServer.External.Database.Rows
{
    public class DbAccount : DbRow
    {
        public DbColumn<int> Id { get; private set; }
        public DbColumn<string> Username { get; set; }
        public DbColumn<string> Email { get; set; }
        public DbColumn<string> Password { get; set; }
        public DbColumn<long> RegistrationTime { get; set; }
        public DbColumn<long> LastActivityTime { get; set; }

        public DbAccount(int id, string username, string email, string password, long registrationTime, long lastActivityTime)
        {
            Id = new("id", id, false);
            Username = new("username", username);
            Email = new("email", email);
            Password = new("password", password);
            RegistrationTime = new("registration_time", registrationTime);
            LastActivityTime = new("last_activity_time", lastActivityTime);
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, Username, Email, Password, RegistrationTime, LastActivityTime];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
