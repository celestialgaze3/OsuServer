using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using System.Numerics;

namespace OsuServer.State
{
    public class OnlinePlayer : Player
    {
        // This player's unique token used to identify their requests ("osu-token" in headers)
        public Connection Connection { get; private set; }
        public string Username { get; private set; }
        public Bancho Bancho { get; private set; }
        public Presence Presence { get; private set; }
        public Status Status { get; private set; }
        public Privileges Privileges { get; private set; }

        // "Block private messages from non-friends" option
        public bool BlockingStrangerMessages { get; set; }

        public Dictionary<GameMode, PlayerStats> Stats { get; private set; } 
            
        public LoginData LoginData { get; private set; }
        public List<Channel> Channels { get; private set; }
        public DateTime LoginTime { get; private set; }
        public Dictionary<GameMode, PlayerScores> Scores { get; private set; }

        public OnlinePlayer(int id, Bancho bancho, Connection connection, LoginData loginData)
            : base(id)
        {
            Bancho = bancho;
            Username = loginData.Username;
            Connection = connection;
            Privileges = new Privileges();
            Stats = new Dictionary<GameMode, PlayerStats>();
            Presence = new Presence();
            Status = new Status();
            Channels = [];
            LoginData = loginData;
            Presence.UtcOffset = loginData.UtcOffset;
            BlockingStrangerMessages = loginData.DisallowPrivateMessages;
            LoginTime = DateTime.Now;
            Scores = new Dictionary<GameMode, PlayerScores>();

            foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
            {
                Stats[gameMode] = new PlayerStats(this, gameMode);
                Scores[gameMode] = new PlayerScores(this, bancho, gameMode);
            }
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

        public async Task UpdateWithScore(OsuServerDb database, SubmittedScore score, Score? previousBestScore)
        {
            Scores[score.GameMode].Add(score, true);
            await Stats[score.GameMode].UpdateWith(database, score, previousBestScore);
        }

        public async Task UpdateFromDb(OsuServerDb database)
        {
            foreach (GameMode gameMode in GameModeHelper.GetMain())
            {
                Console.WriteLine($"Updating player state for gamemode {gameMode.ToString()}");
                // Load existing stats
                await Stats[gameMode].UpdateFromDb(database);
                await Bancho.Scores.UpdateFromDb(database, this, gameMode);

                // Recalculate stats
                await Stats[gameMode].RecalculateStats(database);
                await Stats[gameMode].SaveToDb(database);
            }
        }

        public override Task<string> GetUsername(OsuServerDb database)
        {
            return Task.FromResult(Username);
        }
    }
}
