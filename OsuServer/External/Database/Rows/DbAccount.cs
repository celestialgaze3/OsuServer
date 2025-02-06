namespace OsuServer.External.Database.Rows
{
    public class DbAccount : DbRow
    {
        public DbColumn<int> Id { get; }
        public DbColumn<string> Username { get; }
        public DbColumn<string> Email { get; }
        public DbColumn<string> Password { get;  }
        public DbColumn<long> RegistrationTime { get; }
        public DbColumn<long> LastActivityTime { get; }
        public DbIntArrayColumn Friends { get; }

        public DbAccount(int id, string username, string email, string password, long registrationTime, long lastActivityTime, 
            int[]? friends)
        {
            Id = new("id", id, false);
            Username = new("username", username);
            Email = new("email", email);
            Password = new("password", password);
            RegistrationTime = new("registration_time", registrationTime);
            LastActivityTime = new("last_activity_time", lastActivityTime);
            Friends = new("friends", friends);
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, Username, Email, Password, RegistrationTime, LastActivityTime, Friends];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
