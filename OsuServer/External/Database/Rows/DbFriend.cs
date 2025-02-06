namespace OsuServer.External.Database.Rows
{
    public class DbFriend : DbRow
    {
        public DbColumn<int> Id { get; }
        public DbColumn<int> FriendId { get; }

        public DbFriend(int id, int friendId)
        {
            Id = new("id", id);
            FriendId = new("friend_id", friendId);
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, FriendId];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id, FriendId];
        }
    }
}
