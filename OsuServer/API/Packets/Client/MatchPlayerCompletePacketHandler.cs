using OsuServer.External.Database;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchPlayerCompletePacketHandler : ClientPacketHandler
    {
        public MatchPlayerCompletePacketHandler(byte[] data)
            : base((int)ClientPacketType.MatchPlayerComplete, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Mark the player as complete
            match.MarkAsComplete(player);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} has finished their play in match ID {match.Id}");
        }
    }
}
