using Org.BouncyCastle.Crypto.Digests;
using OsuServer.API;
using OsuServer.API.Packets;
using OsuServer.External.OsuV2Api;
using OsuServer.State;
using OsuServer.Util;
using System.Text;

namespace OsuServer
{
    public class Program
    {

        public static Bancho s_Bancho = new Bancho("Bancho");
        public static BanchoAPI s_BanchoEndpoint = new BanchoAPI(s_Bancho);
        public static OsuApiClient ApiClient { get; private set; }
        public static async Task Main(string[] args)
        {
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
            
            app.Run();
        }

        public static async Task<IResult> Handle(HttpContext context)
        {
            await context.Response.WriteAsync($"{s_Bancho.Name} is up and running!");
            return Results.Ok();
        }

    }
}
