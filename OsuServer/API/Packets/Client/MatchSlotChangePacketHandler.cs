using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchSlotChangePacketHandler : ClientPacketHandler
    {
        public MatchSlotChangePacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchSlotChange, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int slotId = reader.ReadInt32();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Ensure destination slot exists
            MatchSlot? destination = match.GetSlot(slotId);
            if (destination == null) return;

            // Change slots
            match.PlayerChangeSlots(player, destination);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} changed their slot to {slotId} in match ID {match.Id}");
        }
    }
}
