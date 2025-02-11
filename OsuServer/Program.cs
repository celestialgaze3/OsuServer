using MySqlConnector;
using OsuServer.API;
using OsuServer.API.Packets;
using OsuServer.External.Database;
using OsuServer.External.OsuV2Api;
using OsuServer.State;

namespace OsuServer
{
    public class Program
    {
        private static bool _tablesInitialized = false;

        public static Bancho s_Bancho;
        public static BanchoAPI s_BanchoEndpoint;
        public static OsuApiClient ApiClient { get; private set; }
        public static async Task Main(string[] args)
        {

            s_Bancho = new Bancho("Bancho");
            s_BanchoEndpoint = new BanchoAPI(s_Bancho);

            ClientPacketHandler.RegisterPacketTypes();

            // Connect to osu! API (used to retrieve beatmap data)
            Console.WriteLine("Connecting to the osu! API...");
            ApiClient = new(ServerConfiguration.ClientId, ServerConfiguration.ClientSecret);
            await ApiClient.Start();
            Console.WriteLine("Complete!");

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Bancho connections
            app.MapPost("/", (Delegate)s_BanchoEndpoint.HandlePackets);

            // Web connections, mostly for debug purposes to check if the server is online
            app.MapGet("/", (Delegate)Handle);
            app.MapGet("/index.php", (Delegate)Handle);

            // Bancho connect endpoint
            app.MapGet("/web/bancho_connect.php", (Delegate)s_BanchoEndpoint.HandleBanchoConnect);

            // Player avatars (a.ppy.sh/{id})
            app.MapGet("/{id:int}", (Delegate)s_BanchoEndpoint.HandleProfilePictureRequest);

            // Beatmap thumbnails (b.ppy.sh/thumb/{id})
            app.MapGet("/thumb/{id}", (Delegate)s_BanchoEndpoint.HandleBeatmapThumbnailRequest);

            // Score submission
            app.MapPost("/web/osu-submit-modular-selector.php", (Delegate)s_BanchoEndpoint.HandleScoreSubmission);

            // Account registration
            app.MapPost("/users", (Delegate) s_BanchoEndpoint.HandleAccountRegistration);

            // Leaderboard retrieval
            app.MapGet("/web/osu-osz2-getscores.php", (Delegate)s_BanchoEndpoint.HandleLeaderboardRequest);

            // Replay retrieving
            app.MapGet("/web/osu-getreplay.php", (Delegate)s_BanchoEndpoint.HandleReplayRequest);

            app.Run();
        }

        public static async Task<IResult> Handle(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            return Results.Ok($"{s_Bancho.Name} is up and running!");
        }

        public static async Task<OsuServerDb> GetDbConnection()
        {
            var connection = new MySqlConnection($"Server={ServerConfiguration.DatabaseServerIP};" +
                                                 $"User ID={ServerConfiguration.DatabaseUsername};" +
                                                 $"Password={ServerConfiguration.DatabasePassword};" +
                                                 $"Database={ServerConfiguration.DatabaseName};" +
                                                 $"Allow User Variables=True");
            await connection.OpenAsync();

            OsuServerDb database = new(connection);
            if (!_tablesInitialized)
            {
                Console.WriteLine("Initializing tables...");
                await database.InitializeTables();
                Console.WriteLine("Complete!");

                _tablesInitialized = true;
            }
            return database;
        }

    }
}
