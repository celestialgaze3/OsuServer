using OsuServer.External.Database;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchStartPacketHandler : ClientPacketHandler
    {
        public MatchStartPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchStart, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Only the host can start a match
            if (player.Id != match.HostId) return;

            // Start the match
            match.Start();
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} started match ID {match.Id}!");
        }
    }
}
