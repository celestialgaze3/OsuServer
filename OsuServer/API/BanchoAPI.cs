using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using OsuServer.API.Packets;
using OsuServer.API.Packets.Client;
using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.Database.Tables;
using OsuServer.Objects;
using OsuServer.State;
using OsuServer.Util;
using System.Text;
using System.Text.RegularExpressions;

namespace OsuServer.API
{
    public class BanchoAPI
    {
        Bancho Bancho { get; set; }
        public BanchoAPI(Bancho bancho) 
        {
            Bancho = bancho;
        }

        private async Task<IResult> WriteByteResponse(byte[] bytes, HttpResponse response)
        {
            await response.Body.WriteAsync(bytes);
            await response.Body.FlushAsync();
            return Results.Empty;
        }

        public async Task<IResult> HandlePackets(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            // Get the user's IP
            string remoteIp = GetRequestIP(context);

            Console.WriteLine(" - New bancho request from " + remoteIp + " - ");

            /* In all incoming Bancho connections, an "osu-token" is present in the
            headers of the request. The one exception to this is when the client is 
            logging in. */

            string osuToken;

            if (!request.Headers.TryGetValue("osu-token", out StringValues osuTokenHeader))
            {
                // No osu-token, client is attempting to log in
                return await HandleLogin(context);
            } else
            {
                // osu-token is present
                osuToken = osuTokenHeader.ToString();
            }

            OnlinePlayer? player = Bancho.GetPlayer(osuToken);

            // Server has restarted and this client is still logged in
            if (player == null)
            {
                Console.WriteLine("Connection was not set up properly at login, it's likely the server had restarted. Telling client to reconnect.");

                Connection connection = Bancho.CreateConnection(osuToken);
                connection.AddPendingPacket(new ReconnectPacket(1));
                return await WriteByteResponse(connection.FlushPendingPackets(), response);
            }

            Console.WriteLine("Requester is logged in with token " + osuToken);

            /* Note: The issue with sending pending packets only on a client ping is that the client will not receive 
             * responses to actions immediately. This is not too bad considering the previous behavior on things like the 
             * user stats request packet or ping packet, is that the client would respond to the user stats packet or pong 
             * packet that it received in return and respond by sending yet another ping or user stats request packet, 
             * leading to the client/server spamming back and forth constantly. This is obviously not intended behavior, 
             * so it makes me wonder if there is something wrong with the data so the client requests again, or if this 
             * is the intended design of the actual osu!Bancho servers. */

            /* Update: It's not the intended design, something is likely wrong here. Multiplayer packets are much more
             * responsive on the actual servers. We'll allow match packets to be responded to immediately as well to
             * address this for now. */

            // Read request body and handle all client packets within
            List<ClientPacketHandler> packetsHandled = await HandleClientPackets(request, database, osuToken, Bancho);

            // Only respond on a lone client ping
            bool pingOnly = false;
            if (packetsHandled.Count == 1 && packetsHandled[0] is PingPacketHandler)
                pingOnly = true;

            // Or if the packet is a match packet
            bool isMatchPacket = false;
            string? packetName = Enum.GetName<ClientPacketType>((ClientPacketType)packetsHandled[0].Id);
            if (packetsHandled.Count >= 1 && packetName != null && packetName.StartsWith("Match"))
                isMatchPacket = true;

            // TODO: properly address this. maybe simply add problematic spammy packets to a blacklist?
            if (pingOnly || isMatchPacket)
            {
                byte[] pendingPackets = player.Connection.FlushPendingPackets();
                Console.WriteLine($"Received a ping packet, responding with {pendingPackets.Length} bytes of pending packet data");

                // Write pending server packets into response body
                await WriteByteResponse(pendingPackets, response);
            }

            // Get the stored row of this user's account
            DbAccountTable accountTable = database.Account;
            DbAccount? dbAccount = await accountTable.FetchOneAsync(new DbClause("WHERE", "id = @id", new() { ["id"] = player.Id }));

            if (dbAccount == null)
            {
                Console.WriteLine("User seems to be signed into an account that does not exist? Telling client to reconnect.");
                player.Connection.AddPendingPacket(new ReconnectPacket(1));
                return await WriteByteResponse(player.Connection.FlushPendingPackets(), response);
            }

            // Update the user's last activity time
            dbAccount.LastActivityTime.Value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await accountTable.UpdateOneAsync(dbAccount);

            return Results.Empty;
        }

        private async Task<List<ClientPacketHandler>> HandleClientPackets(HttpRequest request, OsuServerDb database, string osuToken, Bancho bancho)
        {
            List<ClientPacketHandler> handlers = [];
            using (var memoryStream = new MemoryStream())
            {
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                foreach (var handler in ClientPacketHandler.ParseIncomingPackets(memoryStream))
                {
                    handlers.Add(handler);
                    await handler.Handle(database, bancho, osuToken);
                }
            }
            return handlers;
        }

        private async Task<IResult> HandleLogin(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine("Client is attempting to login.");

            // Read login data
            string body;
            using (var reader = new StreamReader(request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            LoginData loginData = new LoginData(body);
            loginData.PrintData();

            /*
             * Information that is currently unused:
             * 1. Version (planned): Checking osu! version (disallow versions that are too old)
             * 2. ShouldDisplayCity: I don't know if I will ever implement the geolocation feature (which I'm assuming 
             * this is related to). I've never used it in-game.
             * 3. All of the weird hashes: I assume these are used to uniquely identify computers to assist with multi-
             * account prevention. I am not sure if I care to prevent that with this server, so I may never use these.
             */

            // Now we need to validate the login credentials
            // Get the account they are supposed to be signing into
            DbAccountTable accountTable = database.Account;
            DbAccount? account = await accountTable.FetchOneAsync(
                new DbClause("WHERE", "username = @username", new() { ["username"] = loginData.Username })
            );

            // Ensure account exists and password matches
            if (account == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, account.Password.Value))
            {
                response.Headers.Append("cho-token", "incorrect-credentials");
                Connection failedConnection = Bancho.TokenlessConnection("incorrect-credentials");
                failedConnection.AddPendingPacket(new UserIdPacket((int)LoginFailureType.AuthenticationFailed));
                return await WriteByteResponse(failedConnection.FlushPendingPackets(), response);
            }

            // Disallow this player from logging in again
            /* TODO: Timeout sessions. If a player's game crashes before sending the logout packet, 
             * this will prevent a player from logging in again until a server restart. */
            if (Bancho.GetPlayer(account.Id.Value) != null)
            {
                /* Sometimes the client will try to login again before the first login request is fully processed.
                 * We don't need to do anything here. */
                return Results.Ok();
            }

            // Determine the user's geolocation
            Geolocation geolocation = await Geolocation.Retrieve(context, GetRequestIP(context));

            // Update missing geolocation information if it was not successfully determined at registration
            if (account.CountryCodeNum.ValueIsNull)
            {
                if (!geolocation.CountryCodeIsNull)
                    account.CountryCodeNum.Value = (int)geolocation.CountryCode;

                await accountTable.UpdateOneAsync(account);
            }

            // We now need to send all of the information the client needs to be convinced it's fully connected.

            // Let's start by assigning an osu-token to this client.
            string osuToken = Guid.NewGuid().ToString();
            response.Headers.Append("cho-token", osuToken);

            // Create a connection and player instances for this client
            Connection connection = Bancho.CreateConnection(osuToken);
            OnlinePlayer player = await Bancho.CreatePlayer(database, account.Id.Value, connection, geolocation, loginData);
            await player.EnsureLoaded(database);

            // We now need to send packet information: starting with the protocol version. This is always 19.
            connection.AddPendingPacket(new ProtocolVersionPacket(19));

            // We also need to tell the client its ID.
            connection.AddPendingPacket(new UserIdPacket(player.Id));

            // The privileges the client has (supporter, admin, etc)
            connection.AddPendingPacket(new PrivilegesPacket(player.Privileges.IntValue));

            // Welcome to Bancho! notification, mostly for debug purposes.
            connection.AddPendingPacket(new NotificationPacket($"Welcome to {Bancho.Name}!"));

            /* Send the information of the available channels which are set to auto-join on connect.
             * The client will attempt to join these. */
            foreach (Channel channel in Bancho.GetChannels())
            {
                if (!channel.ShouldAutoJoin || !channel.CanJoin(player)) continue;
                channel.SendInfo(player);
            }

            // Let the client know we're done sending channel information.
            connection.AddPendingPacket(new EndChannelInfoPacket());

            // Send the client their friends list
            connection.AddPendingPacket(new FriendsListPacket(player.Friends));

            // TODO: Silence packet

            // Send the client their own user information
            connection.AddPendingPacket(new UserPresencePacket(player));
            connection.AddPendingPacket(new UserStatsPacket(player));

            // Flush pending server packets into response body
            byte[] data = connection.FlushPendingPackets();
            Console.WriteLine("Response data: " + BitConverter.ToString(data));
            return await WriteByteResponse(data, response);
        }

        public async Task<IResult> HandleWeb(HttpContext context)
        {
            return Results.Ok($"{Bancho.Name} is up and running!");
        }

        public async Task<IResult> HandleBanchoConnect(HttpContext context)
        {
            /* The client sends various information about itself when it
            tries to connect. This includes the GET variables:
            1. "v" (version) - The osu! client's version, always sent
            2. "u" (username) - Username when logging in
            3. "h" (password hash) - Password hashed with MD5
            4. "fx" (.NET Framework versions) - list of .NET Framework versions for some reason
            5. "ch" (client hash) - The client's hash, don't know what that would be used for
            6. "retry" (retrying) - 0 or 1. If the client is retrying the connection, also don't know what that would
            be used for
            */

            Console.WriteLine("Received bancho connect request from " + GetRequestIP(context));

            // TODO: do something with the information

            // Acknowledge request
            return Results.Ok();
        }

        public async Task<IResult> HandleProfilePictureRequest(HttpContext context, int id)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine($"Received avatar request for user ID {id} from {GetRequestIP(context)}");

            HttpClient client = new HttpClient();
            Console.WriteLine($"Downloading data from a.ppy.sh/{id}...");
            try
            {
                byte[] profilePicture = await client.GetByteArrayAsync($"https://a.ppy.sh/{id}");
                Console.WriteLine($"Sending data in response...");
                return await WriteByteResponse(profilePicture, response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Returning 404 Not Found as osu! servers returned the following: {e.Message}");
                return Results.NotFound();
            }
        }
        public async Task<IResult> HandleBeatmapThumbnailRequest(HttpContext context, string filename)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine($"Received beatmap thumbnail request for thumbnail {filename} from {GetRequestIP(context)}");

            HttpClient client = new HttpClient();
            Console.WriteLine($"Downloading data from b.ppy.sh/thumb/{filename}...");

            try
            {
                byte[] thumbnail = await client.GetByteArrayAsync($"https://b.ppy.sh/thumb/{filename}");
                Console.WriteLine($"Sending data in response...");
                return await WriteByteResponse(thumbnail, response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Returning 404 Not Found as osu! servers returned the following: {e.Message}");
                return Results.NotFound();
            }
        }

        public async Task<IResult> HandleScoreSubmission(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine($" - New score submission from {GetRequestIP(context)} - ");

            // All parameters of this POST request
            bool exited = request.Form["x"] != 0;
            int failTime = Int32.Parse(request.Form["ft"].ToString());
            string encryptedScoreDataBase64 = request.Form["score"].ToString();
            string visualSettingsBase64 = request.Form["fs"].ToString();
            string beatmapMD5 = request.Form["bmk"].ToString();
            string hardwareIds = request.Form["c1"].ToString();
            int scoreTime = Int32.Parse(request.Form["st"].ToString());
            string passwordMD5 = request.Form["pass"].ToString();
            string osuVersion = request.Form["osuver"].ToString();
            string encryptedClientHashesBase64 = request.Form["s"].ToString();
            string ivBase64 = request.Form["iv"].ToString();
            string token = request.Form["token"].ToString(); // Don't know what this is for but it's really long

            // Replay file
            IFormFile? file = null;
            byte[]? replayBytes = null;
            if (request.Form.Files.Count > 0)
            {
                file = request.Form.Files[0];

                using MemoryStream stream = new();
                await file.CopyToAsync(stream);
                replayBytes = stream.ToArray();
            }

            // Optional parameter
            string storyboardMD5 = "";
            if (request.Form.ContainsKey("sbk"))
            {
                storyboardMD5 = request.Form["sbk"].ToString();
            }

            /* In order to get the score data, we have to decrypt it with AES and convert it from base64. We'll get 
             * both the score data and in the client hash in this way. The necessary tools for decryption are found
             * in "osuver" (part of the key) and iv (base64'd initialization vectors). I couldn't tell you why we
             * have to jump through all of these hoops, maybe peppy wanted to make it hard to make a replay submitter? */

            string[] scoreDataStr;
            string[] clientHashes;

            // Needed for decryption
            byte[] decryptionKey = Encoding.UTF8.GetBytes($"osu!-scoreburgr---------{osuVersion}"); // score burger
            byte[] decryptionIV = Convert.FromBase64String(ivBase64);

            // Convert from base64
            byte[] encryptedScoreData = Convert.FromBase64String(encryptedScoreDataBase64);
            byte[] encryptedClientHashes = Convert.FromBase64String(encryptedClientHashesBase64);

            // Decrypt score data
            byte[] decryptedScoreData = DecryptRijndael256(encryptedScoreData, decryptionKey, decryptionIV);
            scoreDataStr = Encoding.UTF8.GetString(decryptedScoreData).Split(":");

            // Decrypt client hashes
            byte[] decryptedClientHashes = DecryptRijndael256(encryptedClientHashes, decryptionKey, decryptionIV);
            clientHashes = Encoding.UTF8.GetString(decryptedClientHashes).Split(":");

            Console.WriteLine($"Decrypted score data:");
            int i = 0;
            foreach (string scoreDataLine in scoreDataStr)
            {
                i++;
                Console.WriteLine($"[{i}] {scoreDataLine}");
            }

            Console.WriteLine($"Decrypted client hashes:");
            i = 0;
            foreach (string clientHash in clientHashes)
            {
                i++;
                Console.WriteLine($"[{i}] {clientHash}");
            }

            // Parse the data from the score submission
            ScoreData? scoreData = await ScoreData.GetInstance(database, Bancho, passwordMD5, scoreDataStr);
            if (scoreData == null || !(scoreData.Score.Player is OnlinePlayer))
            {
                /* It's like that the client got signed out due to a server restart. Respond with 
                 * an empty message to keep it retrying. */
                Console.WriteLine("Invalid score submission. ");
                return Results.Ok();
            }

            OnlinePlayer player = (OnlinePlayer)scoreData.Score.Player;

            // Beatmap hash (again?)
            string beatmapMD5FromScore = scoreData.BeatmapMD5;

            // Compare score checksums
            string expectedChecksum = scoreData.Score.CalculateChecksum(beatmapMD5, player.Username, osuVersion, scoreData.ClientTime,
                Encoding.UTF8.GetString(decryptedClientHashes), storyboardMD5);
            string clientChecksum = scoreData.Checksum;

            if (clientChecksum != expectedChecksum)
            {
                Console.WriteLine($"{player.Username} has submitted a score with a seemingly invalid checksum! Ignoring this score submission.");
                Console.WriteLine($"Expected checksum: {expectedChecksum}");
                Console.WriteLine($"Client checksum: {clientChecksum}");

                // TODO: some way of logging/notifying that this error occurred

                return Results.Ok();
            }

            /* The score checksum is calculated with the all of the score data and a time, so it can be used to
             * uniquely identify a score. In this way, we can prevent duplicate submissions. */

            // Save old player stats
            ProfileStats oldStats = player.Stats[scoreData.Score.GameMode].Values;

            // Get beatmap information
            BanchoBeatmap? beatmap = await Bancho.GetBeatmap(database, beatmapMD5);
            if (beatmap == null)
            {
                Console.WriteLine($"{player.Username} submitted a score on an unsubmitted map?");
                return Results.Ok();
            }

            // Get best rank before submission
            int previousRank = await DbScore.GetBestRank(database, beatmap.Info.Id, player.Id, scoreData.Score.GameMode);

            // Update server state with this score
            (SubmittedScore, DbScore?, DbScore?[]) submittedScore = await Bancho.Scores.Submit(
                database, player, scoreData.Score, scoreData.Checksum, replayBytes);
            ScoreStats oldBestStats = await ScoreStats.FromDbScores(database, Bancho, submittedScore.Item3);

            // Get rank of submitted score
            int newRank = await DbScore.GetLeaderboardRank(database, submittedScore.Item2, beatmap.Info.Id, scoreData.Score.GameMode);

            // Send data back to client
            ScoreReport report = new(Bancho, beatmap.Info, player, oldStats, player.Stats[scoreData.Score.GameMode].Values,
                oldBestStats, new ScoreStats(submittedScore.Item1));
            string clientResponse = report.GenerateString(previousRank, newRank, scoreData.Checksum);
            return await WriteByteResponse(Encoding.UTF8.GetBytes(clientResponse), response);
        }
        
        // TODO: move this out
        private byte[] DecryptRijndael256(byte[] encrypted, byte[] key, byte[] iv) // why
        {
            RijndaelEngine rijndael = new RijndaelEngine(256);
            CbcBlockCipher cbc = new CbcBlockCipher(rijndael);
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(cbc, new Pkcs7Padding());
            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamIV = new ParametersWithIV(keyParam, iv, 0, 32);

            cipher.Init(false, keyParamIV);

            byte[] outputBytes = new byte[cipher.GetOutputSize(encrypted.Length)];
            int outputLength = cipher.ProcessBytes(encrypted, outputBytes, 0);
            cipher.DoFinal(outputBytes, outputLength);
            return outputBytes;
        }

        public async Task<IResult> HandleAccountRegistration(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            DbAccountTable accountTable = database.Account;

            // All parameters of this POST request
            string username = request.Form["user[username]"].ToString();
            string email = request.Form["user[user_email]"].ToString();
            string password = request.Form["user[password]"].ToString();
            bool check = Int32.Parse(request.Form["check"].ToString()) == 1;

            Dictionary<string, List<string>> errors = new() { 
                ["username"] = new(),
                ["user_email"] = new(),
                ["password"] = new()
            };

            // Trim leading and trailing whitespace (to avoid confusion)
            username = username.Trim();

            // Username cannot be empty or bigger than 15 characters
            if (username.Length == 0 || username.Length > 15)
            {
                errors["username"].Add("Username must be 1-15 characters.");
            }

            // Username must be alphanumeric
            string nonalphanumericRegex = "[^a-zA-Z0-9 -]";
            if (Regex.IsMatch(username, nonalphanumericRegex))
            {
                errors["username"].Add("Username must be alphanumeric.");
            }

            // Usernames must be unique
            if (errors["username"].Count == 0)
            {
                DbAccount? existingAccount = await accountTable.FetchOneAsync(new DbClause("WHERE", "username = @username", new() { ["username"] = username }));
                if (existingAccount != null)
                {
                    errors["username"].Add("Username has already been taken.");
                }
            }

            // E-mail must be in a valid format
            const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (email.Length == 0 || !Regex.IsMatch(email, emailRegex) || email.Length > 50)
            {
                errors["user_email"].Add("E-mail must be valid.");
            }

            // E-mails must be unique
            if (errors["user_email"].Count == 0)
            {
                DbAccount? existingAccount = await accountTable.FetchOneAsync(new DbClause("WHERE", "email = @email", new() { ["email"] = email }));
                if (existingAccount != null)
                {
                    errors["user_email"].Add("E-mail has already been used for an existing account.");
                }
            }

            // Passwords need to be at least 8 characters
            if (password.Length < 8 || password.Length > 50)
            {
                errors["password"].Add("Password must be 8-50 characters.");
            }

            // Determine whether or we encountered errors
            bool hadErrors = false;
            foreach (var entry in errors)
            {
                if (entry.Value.Count > 0)
                {
                    hadErrors = true;
                    break;
                }
            }

            // Send back errors with the user's input
            if (hadErrors) {
                Dictionary<string, string[]> jsonErrors = new();
                foreach (var entry in errors)
                {
                    if (entry.Value.Count > 0) 
                        jsonErrors[entry.Key] = new string[] { string.Join("\n", entry.Value) };
                }

                var formErrors = new
                {
                    form_error = new
                    {
                        user = jsonErrors
                    }
                };

                response.StatusCode = 400;
                await response.WriteAsJsonAsync(formErrors);

                return Results.Empty;
            }

            // Client wants to create an account, not check inputs
            if (!check)
            {
                Console.WriteLine($" - New account registration from {GetRequestIP(context)} - ");
                Console.WriteLine($"Registering new account {username}...");

                // Passwords must be hashed with MD5 beforehand due to osu! hashing passwords on sign-in
                string passwordMd5 = HashUtil.MD5HashAsUTF8(password);

                // Hash the password's md5 with bcrypt
                string passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(passwordMd5);

                // Save registration time
                long registrationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Determine the users's geolocation
                Geolocation location = await Geolocation.Retrieve(context, GetRequestIP(context));

                // Register the account
                DbAccount toInsert = new(-1, username, email, passwordBcrypt, registrationTime, registrationTime,
                    location.CountryCodeIsNull ? null : (int)location.CountryCode);
                int accountId = await accountTable.InsertAsync(toInsert);
                Console.WriteLine($"Created new account {username} with ID {accountId}!");
            }

            return Results.Ok();
        }

        public async Task<IResult> HandleLeaderboardRequest(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine($" - New leaderboard request from {GetRequestIP(context)} - ");

            // All parameters of this GET request
            bool editSongSelect = request.Query["s"] != 0;
            int leaderboardVersion = Int32.Parse(request.Query["vv"].ToString());
            LeaderboardType leaderboardType = (LeaderboardType)Int32.Parse(request.Query["v"].ToString());
            string beatmapMD5 = request.Query["c"].ToString();
            string filename = request.Query["f"].ToString();
            GameMode gamemode = (GameMode)Int32.Parse(request.Query["m"].ToString());
            int mapSetId = Int32.Parse(request.Query["i"].ToString());
            Mods mods = new Mods(Int32.Parse(request.Query["mods"].ToString()));
            string mapHash = request.Query["h"].ToString();
            bool a = request.Query["a"] != 0;
            string username = request.Query["us"].ToString();
            string passwordMD5 = request.Query["ha"].ToString();

            Console.WriteLine($"Parameters: {beatmapMD5}, {username}, {leaderboardType}, {gamemode}, {mods.IntValue}");

            // Get beatmap information
            BanchoBeatmap? beatmap = await Bancho.GetBeatmap(database, beatmapMD5);

            // Unsubmitted beatmaps
            if (beatmap == null)
            {
                return await WriteByteResponse(Encoding.UTF8.GetBytes("-1|false"), response);
            }

            int beatmapId = beatmap.Info.Id;

            // Get player information
            OnlinePlayer? player = Bancho.GetPlayer(username, passwordMD5);

            // Player is signed out
            if (player == null)
            {
                // Wait for eventual reconnect
                return Results.Ok();
            }

            // Get top 50 scores and player's personal best on the beatmap
            GameMode gameMode = player.Status.GameMode;
            CountryCode countryCode = player.Presence.Geolocation.CountryCode;
            DbScore? playerTopScore;
            List<DbScore> topScores;
            long totalScoreCount;
            int scoreRank;

            switch(leaderboardType)
            {
                case LeaderboardType.Global:
                    playerTopScore = await DbScore.GetTopScoreAsync(database, beatmapId, player.Id, gameMode);
                    topScores = await DbScore.GetTopScoresAsync(database, beatmapId, gameMode);
                    scoreRank = await DbScore.GetLeaderboardRank(database, playerTopScore, beatmapId, gameMode);
                    totalScoreCount = await DbScore.GetScoreCountAsync(database, beatmapId, gameMode);
                    break;
                case LeaderboardType.Friends:
                    playerTopScore = await DbScore.GetTopScoreAsync(database, beatmapId, player.Id, gameMode);
                    topScores = await DbScore.GetFriendTopScoresAsync(database, player.Id, beatmapId, gameMode);
                    scoreRank = await DbScore.GetFriendLeaderboardRank(database, player.Id, playerTopScore, beatmapId, gameMode);
                    totalScoreCount = await DbScore.GetFriendScoreCountAsync(database, player.Id, beatmapId, gameMode);
                    break;
                case LeaderboardType.Country:
                    playerTopScore = await DbScore.GetTopScoreAsync(database, beatmapId, player.Id, gameMode);
                    topScores = await DbScore.GetCountryTopScores(database, beatmapId, countryCode, gameMode);
                    scoreRank = await DbScore.GetCountryLeaderboardRank(database, playerTopScore, countryCode, beatmapId, gameMode);
                    totalScoreCount = await DbScore.GetCountryScoreCountAsync(database, beatmapId, countryCode, gameMode);
                    break;
                case LeaderboardType.Mods:
                    playerTopScore = await DbScore.GetTopModdedScoreAsync(database, beatmapId, player.Id, mods, gameMode);
                    topScores = await DbScore.GetModdedTopScoresAsync(database, beatmapId, mods, gameMode);
                    scoreRank = await DbScore.GetModdedLeaderboardRank(database, playerTopScore, beatmapId, mods, gameMode);
                    totalScoreCount = await DbScore.GetModdedScoreCountAsync(database, beatmapId, mods, gameMode);
                    break;
                default:
                    playerTopScore = null;
                    topScores = [];
                    scoreRank = 0;
                    totalScoreCount = 0;
                    break;
            }

            List<string> responseBody =
            [
                // RankStatus|HasOsz2|BeatmapId|BeatmapSetId|ScoreCount|FeaturedArtistTrackId|FeaturedArtistLicense
                $"{beatmap.GetRankStatus().ValueGetScores}|false|{beatmap.Info.Id}|{beatmap.Info.BeatmapSetId}|{totalScoreCount}|0|",
                // Server offset
                beatmap.Info.BeatmapSet != null ? $"{beatmap.Info.BeatmapSet.Offset}" : "0",
                // Beatmap name
                beatmap.Info.BeatmapSet != null ? $"{beatmap.Info.FullName}" : string.Empty,
                // TODO: Average rating
                "0"
            ];

            // No scores on the map, no need to return anything else
            if (topScores.Count == 0)
            {
                return await WriteByteResponse(Encoding.UTF8.GetBytes(string.Join("\n", responseBody)), response);
            }

            // Add personal best score first
            if (playerTopScore != null)
            {
                responseBody.Add(await GetScoreString(database, playerTopScore, scoreRank));
            } else
            {
                responseBody.Add("");
            }

            // Add rest of leaderboard
            int rank = 0;
            foreach (var score in topScores)
            {
                rank++;
                responseBody.Add(await GetScoreString(database, score, rank));
            }

            return await WriteByteResponse(Encoding.UTF8.GetBytes(string.Join("\n", responseBody)), response);
        }

        private async Task<string> GetScoreString(OsuServerDb database, DbScore dbScore, int rank)
        {
            Score score = await Score.Get(database, Bancho, dbScore);
            return $"{dbScore.Id.Value}|{await score.Player.GetUsername(database)}|{score.TotalScore}|{score.MaxCombo}|" +
                $"{score.Bads}|{score.Goods}|{score.Perfects}|{score.Misses}|{score.Katus}|{score.Gekis}|" +
                $"{score.PerfectCombo}|{score.Mods.IntValue}|{score.Player.Id}|{rank}|{score.Timestamp / 1000}|" +
                (dbScore.ReplayData.ValueIsNull ? 0 : 1);
        }

        public async Task<IResult> HandleReplayRequest(HttpContext context)
        {
            using OsuServerDb database = await OsuServerDb.GetNewConnection();
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Console.WriteLine($" - New replay request from {GetRequestIP(context)} - ");

            // All parameters of this GET request
            GameMode gamemode = (GameMode)Int32.Parse(request.Query["m"].ToString());
            int scoreId = Int32.Parse(request.Query["c"].ToString());

            DbScore? score = await database.Score.FetchOneAsync(
                new DbClause("WHERE", "id = @id", new() { ["id"] = scoreId })
            );

            // Replay not found
            if (score == null || score.ReplayData.ValueIsNull)
            {
                return Results.NotFound();
            }

            return Results.File(score.ReplayData.BlobValue);
        }

        private string? GetRequestIP(HttpContext context)
        {
            string? remoteIp = null;

            // First we must check the headers to see if it has forwarded from cloudflare
            if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues cfConnectingIp))
            {
                remoteIp = cfConnectingIp.ToString();
            }
            // Or proxied in some way
            else if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues xForwardedFor))
            {
                // Get the first value (original IP)
                remoteIp = xForwardedFor.ToString().Split(",")[0];
            }
            else if (context.Connection.RemoteIpAddress != null)
            {
                // If no header exists, just use the remote IP
                remoteIp = context.Connection.RemoteIpAddress.ToString();
            }

            return remoteIp;
        }
    }
}
