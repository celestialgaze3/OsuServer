using OsuServer.External.Database;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchPlayerSkipPacketHandler : ClientPacketHandler
    {
        public MatchPlayerSkipPacketHandler(byte[] data)
            : base((int)ClientPacketType.MatchPlayerSkip, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Mark the player as skipped
            match.MarkAsSkipped(player);

            Console.WriteLine($"{player.Username} has skipped in match ID {match.Id}");
        }
    }
}
