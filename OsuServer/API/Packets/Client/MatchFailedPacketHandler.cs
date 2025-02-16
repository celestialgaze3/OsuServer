using OsuServer.External.Database;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchFailedPacketHandler : ClientPacketHandler
    {
        public MatchFailedPacketHandler(byte[] data)
            : base((int)ClientPacketType.MatchFailed, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Tell players in match that this player failed
            match.BroadcastPlayerFail(player);

            Console.WriteLine($"{player.Username} has loaded into match ID {match.Id}");
        }
    }
}
