using OsuServer.External.Database;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchPlayerLoadedPacketHandler : ClientPacketHandler
    {
        public MatchPlayerLoadedPacketHandler(byte[] data)
            : base((int)ClientPacketType.MatchPlayerLoaded, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Mark the player as loaded
            match.MarkAsLoaded(player);

            Console.WriteLine($"{player.Username} has loaded into match ID {match.Id}");
        }
    }
}
