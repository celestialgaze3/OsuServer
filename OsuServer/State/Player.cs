using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class Player
    {
        public int Id { get; private set; }
        private string? _username;
        public List<int> Friends { get; private set; }

        public Player(int id)
        {
            Id = id;
            Friends = [];
        }

        public virtual async Task<string> GetUsername(OsuServerDb database)
        {
            if (_username == null) {
                DbAccount? account = await database.Account.FetchOneAsync(
                    new DbClause(
                        "WHERE", 
                        "id = @id", 
                        new() { 
                            ["id"] = Id 
                        }
                    )
                );

                if (account != null)
                {
                    _username = account.Username.Value;
                } else
                {
                    _username = "Deleted User";
                }
            }
            return _username;
        }

        public void AddFriend(Player player)
        {
            Friends.Add(player.Id);
        }

        public void RemoveFriend(Player player)
        {
            Friends.Remove(player.Id);
        }

        public bool HasFriended(Player player)
        {
            return Friends.Contains(player.Id);
        }

    }
}
