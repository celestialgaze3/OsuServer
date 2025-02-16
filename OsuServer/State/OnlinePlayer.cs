using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.Util;
using System.Diagnostics.CodeAnalysis;

namespace OsuServer.State
{
    public class OnlinePlayer : Player
    {
        // This player's unique token used to identify their requests ("osu-token" in headers)
        public Connection Connection { get; private set; }

        /// <summary>
        /// The username at login. May have incorrect case.
        /// </summary>
        private string _loginUsername;
        public new string Username
        {
            get
            {
                if (base.Username != null)
                    return base.Username;
                return _loginUsername;
            }
        }

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

        /// <summary>
        /// Whether or not this player is in the lobby (multiplayer lobby selection screen)
        /// </summary>
        public bool IsInLobby { get; set; }

        [MemberNotNullWhen(true, nameof(Match))]
        public bool IsInMatch { get; private set; }
        public Match? Match { get; private set; }

        public OnlinePlayer(Player player, Bancho bancho, Connection connection, Geolocation geolocation,
            LoginData loginData) : base(player)
        {
            Bancho = bancho;
            _loginUsername = loginData.Username;
            Connection = connection;
            Privileges = new Privileges();
            Stats = [];
            Presence = new Presence(loginData.UtcOffset, geolocation, PresenceFilter.All);
            Status = new Status();
            Channels = [];
            LoginData = loginData;
            BlockingStrangerMessages = loginData.DisallowPrivateMessages;
            LoginTime = DateTime.Now;
            Scores = [];

            foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
            {
                Stats[gameMode] = new PlayerStats(this, gameMode);
                Scores[gameMode] = new PlayerScores(this, bancho, gameMode);
            }

            Privileges.Supporter = true;
        }

        public void SendMessage(OsuMessage message)
        {
            Connection.AddPendingPacket(new MessagePacket(message));
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

        public async override Task UpdateFromDb(OsuServerDb database)
        {
            await base.UpdateFromDb(database);
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

        public void JoinMatch(Match match)
        {
            if (match.Add(this))
            {
                IsInMatch = true;
                Match = match;
            }
        }

        public bool TryJoinMatch(Match match, string password)
        {
            bool successful = false;
            if (successful = match.TryAdd(this, password))
            {
                IsInMatch = true;
                Match = match;
            }

            return successful;
        }

        public void LeaveMatch()
        {
            if (Match == null) return;
            Match.Remove(this);
            IsInMatch = false;
            Match = null;
        }
    }
}
