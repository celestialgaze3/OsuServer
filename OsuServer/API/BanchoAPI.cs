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
            this.Bancho = bancho;
        }

        public async Task<IResult> HandlePackets(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            // await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("test"));
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

            Player? player = Bancho.GetPlayer(osuToken);

            // Server has restarted and this client is still logged in
            if (player == null)
            {
                Console.WriteLine("Connection was not set up properly at login, it's likely the server had restarted. Telling client to reconnect.");

                Connection connection = Bancho.CreateConnection(osuToken);
                connection.AddPendingPacket(new ReconnectPacket(1, osuToken, Bancho));
                await response.Body.WriteAsync(connection.FlushPendingPackets());

                return Results.Ok();
            }

            Console.WriteLine("Requester is logged in with token " + osuToken);

            /* Note: The issue with sending pending packets only on a client ping is that the client will not receive 
             * responses to actions immediately. This is not too bad considering the previous behavior on things like the 
             * user stats request packet or ping packet, is that the client would respond to the user stats packet or pong 
             * packet that it received in return and respond by sending yet another ping or user stats request packet, 
             * leading to the client/server spamming back and forth constantly. This is obviously not intended behavior, 
             * so it makes me wonder if there is something wrong with the data so the client requests again, or if this 
             * is the intended design of the actual osu!Bancho servers. */

            // Read request body and handle all client packets within
            List<ClientPacketHandler> packetsHandled = await HandleClientPackets(request, osuToken, Bancho);

            // Only respond on a lone client ping
            bool pingOnly = false;
            if (packetsHandled.Count == 1 && packetsHandled[0] is PingPacketHandler)
                pingOnly = true;

            if (pingOnly)
            {
                byte[] pendingPackets = player.Connection.FlushPendingPackets();
                Console.WriteLine($"Received a ping packet, responding with {pendingPackets.Length} bytes of pending packet data");

                // Write pending server packets into response body
                await response.Body.WriteAsync(pendingPackets);
            }

            return Results.Ok();
        }

        private async Task<List<ClientPacketHandler>> HandleClientPackets(HttpRequest request, string osuToken, Bancho bancho)
        {
            List<ClientPacketHandler> handlers = new List<ClientPacketHandler>();
            using (var memoryStream = new MemoryStream())
            {
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                foreach (var handler in ClientPacketHandler.ParseIncomingPackets(memoryStream, osuToken, bancho))
                {
                    handlers.Add(handler);
                    handler.Handle();
                }
            }
            return handlers;
        }

        private async Task<IResult> HandleLogin(HttpContext context)
        {
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
            DbAccountTable accountTable = new(Bancho.DatabaseConnection);
            await accountTable.CreateTableAsync();
            DbAccount? account = await accountTable.FetchOneAsync(
                new DbClause("WHERE", "username = @username", new() { ["username"] = loginData.Username })
            );

            // Ensure account exists and password matches
            if (account == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, account.Password))
            {
                response.Headers.Append("cho-token", "incorrect-credentials");
                Connection failedConnection = Bancho.TokenlessConnection("incorrect-credentials");
                failedConnection.AddPendingPacket(new UserIdPacket((int)LoginFailureType.AuthenticationFailed, "incorrect-credentials", Bancho));
                await response.Body.WriteAsync(failedConnection.FlushPendingPackets());

                return Results.Ok();
            }
            // TODO: disallow multiple sessions

            // We now need to send all of the information the client needs to be convinced it's fully connected.

            // Let's start by assigning an osu-token to this client.
            string osuToken = Guid.NewGuid().ToString();
            response.Headers.Append("cho-token", osuToken);

            // Create a connection and player instances for this client
            Connection connection = Bancho.CreateConnection(osuToken);
            Player player = Bancho.CreatePlayer(account.Id, connection, loginData);

            // We now need to send packet information: starting with the protocol version. This is always 19.
            connection.AddPendingPacket(new ProtocolVersionPacket(19, osuToken, Bancho));

            // We also need to tell the client its ID.
            connection.AddPendingPacket(new UserIdPacket(player.Id, osuToken, Bancho));

            // The privileges the client has (supporter, admin, etc)
            connection.AddPendingPacket(new PrivilegesPacket(player.Privileges.IntValue, osuToken, Bancho));

            // Welcome to Bancho! notification, mostly for debug purposes.
            connection.AddPendingPacket(new NotificationPacket($"Welcome to {Bancho.Name}!", osuToken, Bancho));

            /* Send the information of the available channels which are set to auto-join on connect.
             * The client will attempt to join these. */
            foreach (Channel channel in Bancho.GetChannels())
            {
                if (!channel.ShouldAutoJoin || !channel.CanJoin(player)) continue;
                channel.SendInfo(player);
            }

            // Let the client know we're done sending channel information.
            connection.AddPendingPacket(new EndChannelInfoPacket(osuToken, Bancho));

            // TODO: Friends list packet
            // TODO: Silence packet

            // Send the client their own user information
            connection.AddPendingPacket(new UserPresencePacket(player, osuToken, Bancho));
            connection.AddPendingPacket(new UserStatsPacket(player, osuToken, Bancho));

            // Flush pending server packets into response body
            byte[] data = connection.FlushPendingPackets();
            Console.WriteLine("Response data: " + BitConverter.ToString(data));
            await response.Body.WriteAsync(data);

            return Results.Ok();
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
                await response.Body.WriteAsync(profilePicture);

                return Results.Empty;
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
                await response.Body.WriteAsync(thumbnail);

                return Results.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Returning 404 Not Found as osu! servers returned the following: {e.Message}");
                return Results.NotFound();
            }
        }

        public async Task<IResult> HandleScoreSubmission(HttpContext context)
        {
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
            ScoreData scoreData = new ScoreData(scoreDataStr);

            // Beatmap hash (again?)
            string beatmapMD5FromScore = scoreData.BeatmapMD5;

            /* For some reason, if the client has supporter, a space is appended to the username in the score data.
             * Let's remove it. */
            string username = scoreData.PlayerUsername;
            if (username.Last() == ' ') username = username.Substring(0, username.Length - 1);

            // Get the player instance (since we don't have the osu-token) by the username passwordMD5 combination
            Player? player = Bancho.GetPlayer(username, passwordMD5);

            if (player == null)
            {
                // Client got signed out likely due to a server restart. Respond with an empty message to keep it retrying.
                return Results.Ok();
            }

            // TODO validate all checksums
            
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
            ProfileStats oldStats = player.Stats.Values;

            // Get beatmap information
            BanchoBeatmap beatmap = await Bancho.GetBeatmap(beatmapMD5);

            // Update server state with this score
            SubmittedScore submittedScore = Bancho.Scores.Submit(player, scoreData.Score, scoreData.Checksum);
            ScoreStats oldBestStats = beatmap.UpdateWithScore(player, submittedScore);

            // Send data back to client
            ScoreReport report = new ScoreReport(Bancho, beatmap.Info, player, oldStats, player.Stats.Values,
                oldBestStats, new ScoreStats(submittedScore));
            string clientResponse = report.GenerateString(scoreData.Checksum);

            await response.Body.WriteAsync(Encoding.UTF8.GetBytes(clientResponse));

            return Results.Ok();
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
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            DbAccountTable accountTable = new(Bancho.DatabaseConnection);
            await accountTable.CreateTableAsync();

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

                return Results.BadRequest();
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

                // Register the account
                DbAccount toInsert = new(username, email, passwordBcrypt);
                int accountId = await accountTable.InsertAsync(toInsert);
                Console.WriteLine($"Created new account {username} with ID {accountId}!");

                // TODO: set the account's geolocation based on their registration IP
            }

            return Results.Ok();
        }

        private string GetRequestIP(HttpContext context)
        {
            string remoteIp;

            // First we must check the headers to see if it has been proxied
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues xForwardedFor))
            {
                // Get the first value (original IP)
                remoteIp = xForwardedFor.ToString().Split(",")[0];
            }
            else if (context.Connection.RemoteIpAddress != null)
            {
                // If no header exists, just use the remote IP
                remoteIp = context.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                // If somehow we still don't have an IP
                remoteIp = "Unknown";
            }

            return remoteIp;
        }
    }
}
