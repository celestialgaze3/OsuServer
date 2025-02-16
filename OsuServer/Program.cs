using OsuServer.API;
using OsuServer.API.Packets;
using OsuServer.External.OsuV2Api;
using OsuServer.State;

namespace OsuServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ClientPacketHandler.RegisterPacketTypes();

            // Connect to osu! API (used to retrieve beatmap data)
            Console.WriteLine("Connecting to the osu! API...");
            OsuApiClient ApiClient = new(ServerConfiguration.ClientId, ServerConfiguration.ClientSecret);
            await ApiClient.Start();
            Console.WriteLine("Complete!");

            Bancho bancho = new("Bancho", ApiClient);
            BanchoAPI banchoEndpoint = new(bancho);

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Bancho connections
            app.MapPost("/", (Delegate)banchoEndpoint.HandlePackets);

            // Web connections, mostly for debug purposes to check if the server is online
            app.MapGet("/", (Delegate)banchoEndpoint.HandleWeb);
            app.MapGet("/index.php", (Delegate)banchoEndpoint.HandleWeb);

            // Bancho connect endpoint
            app.MapGet("/web/bancho_connect.php", (Delegate)banchoEndpoint.HandleBanchoConnect);

            // Player avatars (a.ppy.sh/{id})
            app.MapGet("/{id:int}", banchoEndpoint.HandleProfilePictureRequest);

            // Beatmap thumbnails (b.ppy.sh/thumb/{filename})
            app.MapGet("/thumb/{filename}", banchoEndpoint.HandleBeatmapThumbnailRequest);

            // Score submission
            app.MapPost("/web/osu-submit-modular-selector.php", (Delegate)banchoEndpoint.HandleScoreSubmission);

            // Account registration
            app.MapPost("/users", (Delegate) banchoEndpoint.HandleAccountRegistration);

            // Leaderboard retrieval
            app.MapGet("/web/osu-osz2-getscores.php", (Delegate)banchoEndpoint.HandleLeaderboardRequest);

            // Replay retrieving
            app.MapGet("/web/osu-getreplay.php", (Delegate)banchoEndpoint.HandleReplayRequest);

            app.Run();
        }

    }
}
