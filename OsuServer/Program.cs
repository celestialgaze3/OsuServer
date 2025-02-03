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
            app.MapPost("/", async (context) => await s_BanchoEndpoint.HandlePackets(context));

            // Web connections, mostly for debug purposes to check if the server is online
            app.MapGet("/", async (context) => await Handle(context));
            app.MapGet("/index.php", async (context) => await Handle(context));

            // Bancho connect endpoint
            app.MapGet("/web/bancho_connect.php", async (context) => await s_BanchoEndpoint.HandleBanchoConnect(context));

            // Player avatars (a.ppy.sh/{id})
            app.MapGet("/{id:int}", async (HttpContext context, int id) => await s_BanchoEndpoint.HandleProfilePictureRequest(context, id));

            // Beatmap thumbnails (b.ppy.sh/thumb/{id})
            app.MapGet("/thumb/{id}", async (HttpContext context, string id) => await s_BanchoEndpoint.HandleBeatmapThumbnailRequest(context, id));

            // Score submission
            app.MapPost("/web/osu-submit-modular-selector.php", async (HttpContext context) => await s_BanchoEndpoint.HandleScoreSubmission(context));

            // Account registration
            app.MapPost("/users", async (HttpContext context) => await s_BanchoEndpoint.HandleAccountRegistration(context));

            // Score submission
            app.MapGet("/web/osu-osz2-getscores.php", async (HttpContext context) => await s_BanchoEndpoint.HandleLeaderboardRequest(context));

            app.Run();
        }

        public static async Task<IResult> Handle(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            await response.WriteAsync($"{s_Bancho.Name} is up and running!");
            return Results.Ok();
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
