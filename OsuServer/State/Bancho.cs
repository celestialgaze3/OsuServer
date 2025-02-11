using OsuServer.API;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api.Requests;
using OsuServer.External.OsuV2Api.Responses;
using OsuServer.Util;

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

        public async Task<OnlinePlayer> CreatePlayer(OsuServerDb database, int id,  Connection connection, Geolocation geolocation,
            LoginData data)
        {
            if (_tokenToPlayerId.ContainsKey(connection.Token)) return _playerIdToPlayer[_tokenToPlayerId[connection.Token]];

            Player player = new(id);
            await player.UpdateFromDb(database);
            OnlinePlayer onlinePlayer = new(player, this, connection, geolocation, data);
            await OnPlayerConnect(database, onlinePlayer);

            _tokenToPlayerId[connection.Token] = onlinePlayer.Id;
            _nameToPlayerId[await onlinePlayer.GetUsername(database)] = onlinePlayer.Id;
            _playerIdToPlayer[onlinePlayer.Id] = onlinePlayer;

            // Map username + passwords to player IDs
            _usernamePasswordMD5ToPlayerId[await onlinePlayer.GetUsername(database) + "#" + onlinePlayer.LoginData.Password] = onlinePlayer.Id;
            return onlinePlayer;
        }

        public async Task<BanchoBeatmap?> GetBeatmap(OsuServerDb database, string? beatmapMD5 = null, int? beatmapId = null)
        {
            Console.WriteLine($"Retrieving beatmap with search params " +
                (beatmapMD5 != null ? $"MD5 {beatmapMD5}" : "") + (beatmapId != null ? $"ID {beatmapId}" : ""));

            if (beatmapMD5 == null && beatmapId == null)
                throw new InvalidOperationException("A beatmap's hash or ID must be provided.");

            // Retrieve from either cache if it already exists
            if (beatmapMD5 != null && _beatmapMD5ToBeatmapId.ContainsKey(beatmapMD5))
            {
                Console.WriteLine("Found beatmap in cache!");
                return _beatmapIdToBeatmap[_beatmapMD5ToBeatmapId[beatmapMD5]];
            }
            if (beatmapId != null && _beatmapIdToBeatmap.ContainsKey((int)beatmapId))
            {
                Console.WriteLine("Found beatmap in cache!");
                return _beatmapIdToBeatmap[(int)beatmapId];
            }

            // Retrieve from database if we have already looked up this beatmap before
            Console.WriteLine("Beatmap not found in cache. Searching database...");
            DbClause searchClause = beatmapId != null ?
                new DbClause("WHERE", "id = @id", new() { ["id"] = beatmapId }) :
                new DbClause("WHERE", 
                    "checksum = @checksum", 
                    new() { ["checksum"] = beatmapMD5}
                );
            DbBeatmap? dbBeatmap = await database.Beatmap.FetchOneAsync(searchClause);
            if (dbBeatmap != null)
            {
                Console.WriteLine("Found beatmap in database!");
                return new BanchoBeatmap(this, dbBeatmap.BeatmapExtended);
            }

            // Query osu! API for beatmap information
            Console.WriteLine("Beatmap not found in database. Querying osu! API...");
            BeatmapLookupResponse? response = await Program.ApiClient.SendRequest(new BeatmapLookupRequest(beatmapMD5, null, beatmapId.ToString()));

            // Unsubmitted beatmaps
            if (response == null)
            {
                Console.WriteLine("Beatmap is unsubmitted.");
                return null;
            }
            
            int id = response.BeatmapExtended.Id;

            BanchoBeatmap beatmap = new(this, response.BeatmapExtended);

            // Save to caches
            if (beatmapMD5 != null) 
                _beatmapMD5ToBeatmapId.Add(beatmapMD5, id);
            _beatmapIdToBeatmap.Add(id, beatmap);

            // Save to database to avoid osu! API spam on server restarts
            await database.Beatmap.InsertAsync(new DbBeatmap(response.BeatmapExtended));

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
            if (_playerIdToPlayer.TryGetValue(id, out OnlinePlayer? value)) 
                return value;

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
            // Updates this player's state from the database
            await player.UpdateFromDb(database);

            foreach (OnlinePlayer onlinePlayer in GetPlayers())
            {
                // Send each online player a copy of this user's info
                onlinePlayer.Connection.AddPendingPacket(new UserPresencePacket(player, onlinePlayer.Connection.Token, this));
                onlinePlayer.Connection.AddPendingPacket(new UserStatsPacket(player, onlinePlayer.Connection.Token, this));

                // Send the connecting player a copy of all online user's info
                player.Connection.AddPendingPacket(new UserPresencePacket(onlinePlayer, player.Connection.Token, this));
                player.Connection.AddPendingPacket(new UserStatsPacket(onlinePlayer, player.Connection.Token, this));
            }
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
