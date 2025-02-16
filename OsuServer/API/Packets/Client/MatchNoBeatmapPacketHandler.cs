using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchNoBeatmapPacketHandler : MatchChangeSlotStatusPacketHandler
    {
        public MatchNoBeatmapPacketHandler(byte[] data) 
            : base(MatchSlot.SlotStatus.NoMap, (int) ClientPacketType.MatchNoBeatmap, data) { }

    }
}
