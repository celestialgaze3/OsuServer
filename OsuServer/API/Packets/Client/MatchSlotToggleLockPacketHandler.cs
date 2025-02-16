using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchSlotToggleLockPacketHandler : ClientPacketHandler
    {
        public MatchSlotToggleLockPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchSlotToggleLock, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int slotId = reader.ReadInt32();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;
            if (!player.IsInMatch) return Task.CompletedTask;
            Match match = player.Match;

            // Player must be host to lock slots
            if (player.Id != match.HostId)
                return Task.CompletedTask;

            // Ensure target slot exists
            MatchSlot? targetSlot = match.GetSlot(slotId);
            if (targetSlot == null) return Task.CompletedTask;

            match.ToggleLock(targetSlot);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} locked slot {slotId} in match ID {match.Id}");
            return Task.CompletedTask;
        }
    }
}