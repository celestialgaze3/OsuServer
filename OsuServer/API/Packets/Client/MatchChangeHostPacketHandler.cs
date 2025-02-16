using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchChangeHostPacketHandler : ClientPacketHandler
    {
        public MatchChangeHostPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchChangeHost, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int slotId = reader.ReadInt32();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Only the host can change host (obviously)
            if (player.Id != match.HostId) return;

            // Handle this mod change
            match.ChangeHost(slotId);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} changed the host match ID {match.Id} to slot ID {slotId}");
        }
    }
}
