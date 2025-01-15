using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.Objects;
using System.Numerics;

namespace OsuServer.State
{
    public class Bancho
    {
        public string Name { get; set; }

        private Dictionary<string, int> PlayerTokenMap = new();
        private Dictionary<string, int> PlayerNameMap = new();
        private Dictionary<string, int> PlayerUsernamePasswordMD5Map = new(); // For identifying players from submitted scores
        private Dictionary<int, Player> PlayerIDMap = new();
        private Dictionary<string, Connection> ConnectionTokenMap = new();
        private Dictionary<string, Channel> ChannelNameMap = new();
        private Dictionary<string, Score> ScoreChecksumMap = new();

        // TODO: Temporary score ID assignment, implement this with database functionality
        private Dictionary<string, int> ScoreIdMap = new();
        private int _latestScoreId = 0;

        //private Dictionary<Beatmap, List<Score>> BeatmapScoreListMap = new();

        public Bancho(string name)
        {
            Name = name;

            CreateDefaultChannels();
        }

        private void CreateDefaultChannels()
        {
            CreateChannel("osu", "Default channel").AutoJoin();
            CreateChannel("osu2", "Default channel2").AutoJoin();
        }

        public List<Player> GetPlayers()
        {
            return PlayerIDMap.Values.ToList();
        }

        public List<Channel> GetChannels()
        {
            return ChannelNameMap.Values.ToList();
        }

        public Connection CreateConnection(string token)
        {
            if (ConnectionTokenMap.ContainsKey(token)) return ConnectionTokenMap[token];

            Connection connection = new Connection(token, this);
            ConnectionTokenMap.Add(token, connection);
            return connection;
        }

        public Player CreatePlayer(Connection connection, LoginData data)
        {
            if (PlayerTokenMap.ContainsKey(connection.Token)) return PlayerIDMap[PlayerTokenMap[connection.Token]];

            Player player = new Player(connection, data);
            OnPlayerConnect(player);
            PlayerTokenMap.Add(connection.Token, player.Id);
            PlayerNameMap.Add(player.Username, player.Id);
            PlayerIDMap.Add(player.Id, player);

            // Map username + passwords to player IDs
            PlayerUsernamePasswordMD5Map.Add(player.Username + "#" + player.LoginData.Password, player.Id);
            return player;
        }

        public Channel CreateChannel(string name, string description)
        {
            if (ChannelNameMap.ContainsKey(name)) return ChannelNameMap[name];

            Channel channel = new Channel(name, description, this);
            ChannelNameMap.Add(name, channel);
            return channel;
        }

        public int GetScoreId(string scoreChecksum)
        {
            return ScoreIdMap[scoreChecksum];
        }

        public void SubmitScore(Player player, Score score, string scoreChecksum)
        {
            if (IsScoreSubmitted(scoreChecksum)) return;
            ScoreChecksumMap.Add(scoreChecksum, score);

            // TODO: track ids properly
            ScoreIdMap.Add(scoreChecksum, _latestScoreId);
            _latestScoreId++;

            // TODO: update player state properly
            player.Stats.Playcount += 1;
            player.Stats.TotalScore += score.TotalScore;
            if (score.Passed) player.Stats.RankedScore += score.TotalScore;
            player.Stats.Rank = score.Goods;
        }

        public bool IsScoreSubmitted(string checksum)
        {
            return ScoreChecksumMap.ContainsKey(checksum);
        }

        public Connection? GetConnection(string token)
        {
            if (!ConnectionTokenMap.ContainsKey(token)) return null;
            return ConnectionTokenMap[token];
        }

        public Player? GetPlayer(string token)
        {
            if (!PlayerTokenMap.ContainsKey(token)) return null;
            return PlayerIDMap[PlayerTokenMap[token]];
        }

        public Player? GetPlayer(int id)
        {
            if (!PlayerIDMap.ContainsKey(id)) return null;
            return PlayerIDMap[id];
        }

        public Player? GetPlayer(string username, string passwordMD5)
        {
            string key = username + "#" + passwordMD5;
            if (!PlayerUsernamePasswordMD5Map.ContainsKey(key)) return null;
            return PlayerIDMap[PlayerUsernamePasswordMD5Map[key]];
        }

        public Player? GetPlayerByUsername(string name)
        {
            if (!PlayerNameMap.ContainsKey(name)) return null;
            return PlayerIDMap[PlayerNameMap[name]];
        }

        public Channel? GetChannel(string name)
        {
            if (!ChannelNameMap.ContainsKey(name)) return null;
            return ChannelNameMap[name];
        }

        public void OnPlayerConnect(Player player)
        {
            foreach(Player onlinePlayer in GetPlayers())
            {
                // Send each online player a copy of this user's info
                onlinePlayer.Connection.AddPendingPacket(new UserPresencePacket(player, onlinePlayer.Connection.Token, this));
                onlinePlayer.Connection.AddPendingPacket(new UserStatsPacket(player, onlinePlayer.Connection.Token, this));

                // Send the connecting player a copy of all online user's info
                player.Connection.AddPendingPacket(new UserPresencePacket(onlinePlayer, player.Connection.Token, this));
                player.Connection.AddPendingPacket(new UserStatsPacket(onlinePlayer, player.Connection.Token, this));
            }
        }

        public void BroadcastUserUpdate(Player player)
        {
            foreach (Player onlinePlayer in GetPlayers())
            {
                onlinePlayer.Connection.AddPendingPacket(new UserStatsPacket(player, onlinePlayer.Connection.Token, this));
            }
        }

        public void RemovePlayer(Player player)
        {
            PlayerTokenMap.Remove(player.Connection.Token);
            PlayerIDMap.Remove(player.Id);

            // Remove the player's internal state from bancho
            player.ClearState();

            // Broadcast logout
            foreach (Player onlinePlayer in GetPlayers())
            {
                onlinePlayer.Connection.AddPendingPacket(new LogoutPacket(player.Id, onlinePlayer.Connection.Token, this));
            }
        }

    }
}
