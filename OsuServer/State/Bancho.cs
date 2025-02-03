using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.External.OsuV2Api.Requests;
using OsuServer.External.OsuV2Api.Responses;

namespace OsuServer.State
{
    public class Bancho
    {
        public string Name { get; set; }
        public BanchoScores Scores { get; private set; }

        private Dictionary<string, int> _tokenToPlayerId = [];
        private Dictionary<string, int> _nameToPlayerId = [];
        private Dictionary<string, int> _usernamePasswordMD5ToPlayerId = []; // For identifying players from submitted scores
        private Dictionary<int, OnlinePlayer> _playerIdToPlayer = [];
        private Dictionary<string, Connection> _tokenToConnection = [];

        private Dictionary<string, Channel> _nameToChannel = [];

        private Dictionary<int, BanchoBeatmap> _beatmapIdToBeatmap = [];
        private Dictionary<string, int> _beatmapMD5ToBeatmapId = [];

        public Bancho(string name)
        {
            Name = name;
            Scores = new BanchoScores(this);

            CreateDefaultChannels();
        }

        private void CreateDefaultChannels()
        {
            CreateChannel("osu", "Default channel").AutoJoin();
            CreateChannel("osu2", "Default channel2").AutoJoin();
        }

        public List<OnlinePlayer> GetPlayers()
        {
            return _playerIdToPlayer.Values.ToList();
        }

        public List<Channel> GetChannels()
        {
            return _nameToChannel.Values.ToList();
        }

        public Connection TokenlessConnection(string mockToken)
        {
            return new(mockToken, this);
        }

        public Connection CreateConnection(string token)
        {
            if (_tokenToConnection.ContainsKey(token)) return _tokenToConnection[token];

            Connection connection = new Connection(token, this);
            _tokenToConnection.Add(token, connection);
            return connection;
        }

        public async Task<OnlinePlayer> CreatePlayer(OsuServerDb database, int id, Connection connection, LoginData data)
        {
            if (_tokenToPlayerId.ContainsKey(connection.Token)) return _playerIdToPlayer[_tokenToPlayerId[connection.Token]];

            OnlinePlayer player = new OnlinePlayer(id, this, connection, data);
            await OnPlayerConnect(database, player);
            _tokenToPlayerId[connection.Token] = player.Id;
            _nameToPlayerId[await player.GetUsername(database)] = player.Id;
            _playerIdToPlayer[player.Id] = player;

            // Map username + passwords to player IDs
            _usernamePasswordMD5ToPlayerId[await player.GetUsername(database) + "#" + player.LoginData.Password] = player.Id;
            return player;
        }

        public async Task<BanchoBeatmap> GetBeatmap(string beatmapMD5)
        {
            // Retrieve from cache if it already exists
            if (_beatmapMD5ToBeatmapId.ContainsKey(beatmapMD5))
            {
                return _beatmapIdToBeatmap[_beatmapMD5ToBeatmapId[beatmapMD5]];
            }

            // Query osu! API for beatmap information
            BeatmapLookupResponse response = await Program.ApiClient.SendRequest(new BeatmapLookupRequest(beatmapMD5, null, null));
            int id = response.BeatmapExtended.Id;

            BanchoBeatmap beatmap = new BanchoBeatmap(this, response.BeatmapExtended);

            _beatmapMD5ToBeatmapId.Add(beatmapMD5, id);
            _beatmapIdToBeatmap.Add(id, beatmap);

            // TODO: Save to database to avoid osu! API spam on server restarts

            return beatmap;
        }

        public async Task<BanchoBeatmap> GetBeatmap(int beatmapId)
        {
            // Retrieve from cache if it already exists
            if (_beatmapIdToBeatmap.ContainsKey(beatmapId))
            {
                return _beatmapIdToBeatmap[beatmapId];
            }

            // Query osu! API for beatmap information
            BeatmapLookupResponse response = await Program.ApiClient.SendRequest(new BeatmapLookupRequest(null, null, beatmapId.ToString()));
            string? beatmapMD5 = response.BeatmapExtended.Checksum;

            BanchoBeatmap beatmap = new BanchoBeatmap(this, response.BeatmapExtended);

            if (beatmapMD5 != null)
            {
                _beatmapMD5ToBeatmapId.Add(beatmapMD5, beatmapId);
            }
            _beatmapIdToBeatmap.Add(beatmapId, beatmap);

            return beatmap;
        }

        public Channel CreateChannel(string name, string description)
        {
            if (_nameToChannel.ContainsKey(name)) return _nameToChannel[name];

            Channel channel = new Channel(name, description, this);
            _nameToChannel.Add(name, channel);
            return channel;
        }

        public Connection? GetConnection(string token)
        {
            if (!_tokenToConnection.ContainsKey(token)) return null;
            return _tokenToConnection[token];
        }

        public OnlinePlayer? GetPlayer(string token)
        {
            if (!_tokenToPlayerId.ContainsKey(token)) return null;
            return _playerIdToPlayer[_tokenToPlayerId[token]];
        }

        public OnlinePlayer? GetPlayer(int id)
        {
            if (!_playerIdToPlayer.ContainsKey(id)) return null;
            return _playerIdToPlayer[id];
        }

        public Player GetOfflinePlayer(int id)
        {
            if (_playerIdToPlayer.ContainsKey(id)) 
                return _playerIdToPlayer[id];

            return new Player(id);
        }

        public OnlinePlayer? GetPlayer(string username, string passwordMD5)
        {
            string key = username + "#" + passwordMD5;
            if (!_usernamePasswordMD5ToPlayerId.ContainsKey(key)) return null;
            return _playerIdToPlayer[_usernamePasswordMD5ToPlayerId[key]];
        }

        public OnlinePlayer? GetPlayerByUsername(string name)
        {
            if (!_nameToPlayerId.ContainsKey(name)) return null;
            return _playerIdToPlayer[_nameToPlayerId[name]];
        }

        public Channel? GetChannel(string name)
        {
            if (!_nameToChannel.ContainsKey(name)) return null;
            return _nameToChannel[name];
        }

        public async Task OnPlayerConnect(OsuServerDb database, OnlinePlayer player)
        {
            foreach(OnlinePlayer onlinePlayer in GetPlayers())
            {
                // Send each online player a copy of this user's info
                onlinePlayer.Connection.AddPendingPacket(new UserPresencePacket(player, onlinePlayer.Connection.Token, this));
                onlinePlayer.Connection.AddPendingPacket(new UserStatsPacket(player, onlinePlayer.Connection.Token, this));

                // Send the connecting player a copy of all online user's info
                player.Connection.AddPendingPacket(new UserPresencePacket(onlinePlayer, player.Connection.Token, this));
                player.Connection.AddPendingPacket(new UserStatsPacket(onlinePlayer, player.Connection.Token, this));
            }

            await player.UpdateFromDb(database);
        }

        public void BroadcastUserUpdate(OnlinePlayer player)
        {
            foreach (OnlinePlayer onlinePlayer in GetPlayers())
            {
                onlinePlayer.Connection.AddPendingPacket(new UserStatsPacket(player, onlinePlayer.Connection.Token, this));
            }
        }

        public void RemovePlayer(OnlinePlayer player)
        {
            _tokenToPlayerId.Remove(player.Connection.Token);
            _playerIdToPlayer.Remove(player.Id);

            // Remove the player's internal state from bancho
            player.ClearState();

            // Broadcast logout
            foreach (OnlinePlayer onlinePlayer in GetPlayers())
            {
                onlinePlayer.Connection.AddPendingPacket(new LogoutPacket(player.Id, onlinePlayer.Connection.Token, this));
            }
        }

    }
}
