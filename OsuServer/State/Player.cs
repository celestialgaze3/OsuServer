using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class Player
    {
        // This player's unique token used to identify their requests ("osu-token" in headers)
        public Connection Connection { get; private set; }
        public Bancho Bancho { get; private set; }
        public int Id { get; private set; }
        public string Username { get; private set; }
        public Privileges Privileges { get; private set; }
        public PlayerStats Stats { get; private set; }
        public Presence Presence { get; private set; }
        public Status Status { get; private set; }
        public List<int> Friends { get; private set; }

        // "Block private messages from non-friends" option
        public bool BlockingStrangerMessages { get; set; }

        public LoginData LoginData { get; private set; }
        public List<Channel> Channels { get; private set; }
        public DateTime LoginTime { get; private set; }
        public PlayerScores Scores { get; private set; }

        public Player(int id, Bancho bancho, Connection connection, LoginData loginData)
        {
            Id = id;
            Bancho = bancho;
            Connection = connection;
            Username = loginData.Username;
            Privileges = new Privileges();
            Stats = new PlayerStats(this);
            Presence = new Presence();
            Status = new Status();
            Channels = [];
            Friends = [];
            LoginData = loginData;
            Presence.UtcOffset = loginData.UtcOffset;
            BlockingStrangerMessages = loginData.DisallowPrivateMessages;
            LoginTime = DateTime.Now;
            Scores = new PlayerScores(this, Bancho);
        }

        public void SendMessage(OsuMessage message)
        {
            Connection.AddPendingPacket(new MessagePacket(message, Connection.Token, Connection.Bancho));
        }

        public bool JoinChannel(Channel channel)
        {
            if (channel.AddPlayer(this))
            {
                Channels.Add(channel);
                return true;
            }

            return false;
        }

        public void LeaveChannel(Channel channel)
        {
            channel.RemovePlayer(this);
            Channels.Remove(channel);
        }

        public void ClearState()
        {
            foreach (var channel in Channels.ToArray())
            {
                LeaveChannel(channel);
            }
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

        public async Task UpdateWithScore(SubmittedScore score)
        {
            Scores.Add(score);
            await Stats.UpdateWith(score);
        }

        public async Task UpdateFromDb()
        {
            await Stats.UpdateFromDb();
            await Bancho.Scores.UpdateFromDb(this);
        }

    }
}
