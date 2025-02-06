using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using System.Diagnostics.CodeAnalysis;

namespace OsuServer.State
{
    public class Player
    {
        public int Id { get; private set; }


        [MemberNotNullWhen(true, nameof(_username), nameof(Friends), nameof(_loadedFriends))]
        public bool HasLoadedFromDb { get; private set; }
        public DbAccount? DatabaseInstance { get; private set; }

        private string? _username = null;
        private HashSet<int> _loadedFriends = new HashSet<int>();
        public HashSet<int>? Friends { get; private set; } = null;


        public Player(int id)
        {
            Id = id;
        }

        public virtual async Task<string> GetUsername(OsuServerDb database)
        {
            if (_username == null) {
                await UpdateFromDb(database);
            }
            return _username;
        }

        public async Task SaveToDb(OsuServerDb database)
        {
            DbAccount? account = await GetRow(database);

            // It shouldn't be possible to save a player instance of a user that doesn't exist (must be registered)
            if (account == null)
                throw new InvalidOperationException($"The account with ID {Id} does not exist!");

            // It shouldn't be possible to save a player instance if we haven't loaded the previous values before
            if (!HasLoadedFromDb)
                throw new InvalidOperationException($"Saving before loading existing data from the database " +
                    $"will result in saving invalid data.");

            // Update with any new friend changes:
            // New friend insertion
            foreach (var friend in Friends)
            {
                if (_loadedFriends.Contains(friend)) continue;
                await database.Friend.InsertAsync(new DbFriend(Id, friend));
                _loadedFriends.Add(friend);
            }

            // Old friend deletion
            foreach (var friend in _loadedFriends)
            {
                if (Friends.Contains(friend)) continue;
                await database.Friend.DeleteOneAsync(new DbFriend(Id, friend));
                _loadedFriends.Remove(friend);
            }

            await database.Account.UpdateOneAsync(account);
        }

        [MemberNotNull(nameof(_username), nameof(Friends))]
        public virtual async Task UpdateFromDb(OsuServerDb database)
        {
            _username = "Unknown User";
            Friends = [];

            DbAccount? dbAccount = await GetRow(database);
            if (dbAccount != null)
            {
                _username = dbAccount.Username.Value;
                Friends = (await GetFriends(database))
                    .Select(friend => friend.FriendId.Value).ToHashSet();
                _loadedFriends = new HashSet<int>(Friends);
            }

            HasLoadedFromDb = true;
        }

        private async Task<DbAccount?> GetRow(OsuServerDb database)
        {
            if (DatabaseInstance != null) return DatabaseInstance;

            DatabaseInstance = await database.Account.FetchOneAsync(
                new DbClause("WHERE", "id = @id", new() { ["id"] = Id })
            );

            return DatabaseInstance;
        }

        private async Task<List<DbFriend>> GetFriends(OsuServerDb database)
        {
            return await database.Friend.FetchManyAsync(
                new DbClause("WHERE", "id = @id", new() { ["id"] = Id })
            );
        }

        public async Task AddFriend(OsuServerDb database, Player player)
        {
            await EnsureLoaded(database);
            Friends.Add(player.Id);
            await SaveToDb(database);
        }

        public async Task RemoveFriend(OsuServerDb database, Player player)
        {
            await EnsureLoaded(database);
            Friends.Remove(player.Id);
            await SaveToDb(database);
        }

        public async Task<bool> HasFriended(OsuServerDb database, Player player)
        {
            await EnsureLoaded(database);
            return Friends.Contains(player.Id);
        }

        [MemberNotNull(nameof(_username), nameof(Friends))]
        public async Task EnsureLoaded(OsuServerDb database)
        {
            if (!HasLoadedFromDb)
                await UpdateFromDb(database);
        }

    }
}
