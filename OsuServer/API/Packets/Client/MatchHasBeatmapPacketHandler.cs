using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchHasBeatmapPacketHandler : MatchChangeSlotStatusPacketHandler
    {
        public MatchHasBeatmapPacketHandler(byte[] data) 
            : base(MatchSlot.SlotStatus.NotReady, (int) ClientPacketType.MatchHasBeatmap, data) { }
    }
}
