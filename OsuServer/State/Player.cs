using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using System.Diagnostics.CodeAnalysis;

namespace OsuServer.State
{
    public class Player
    {
        public int Id { get; private set; }


        [MemberNotNullWhen(true, nameof(_username), nameof(Friends))]
        public bool HasLoadedFromDb { get; private set; }
        public DbAccount? DatabaseInstance { get; private set; }

        private string? _username = null;
        public List<int>? Friends { get; private set; } = null;


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

            account.Friends.BlobValue = [.. Friends];
            
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
                Friends = !dbAccount.Friends.ValueIsNull ? [.. dbAccount.Friends.BlobValue] : [];
            }

            HasLoadedFromDb = true;
        }

        public async Task<DbAccount?> GetRow(OsuServerDb database)
        {
            if (DatabaseInstance != null) return DatabaseInstance;

            DatabaseInstance = await database.Account.FetchOneAsync(
                new DbClause("WHERE", "id = @id", new() { ["id"] = Id })
            );

            return DatabaseInstance;
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
