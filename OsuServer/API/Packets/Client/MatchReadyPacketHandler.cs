using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchReadyPacketHandler : MatchChangeSlotStatusPacketHandler
    {
        public MatchReadyPacketHandler(byte[] data) 
            : base(MatchSlot.SlotStatus.Ready, (int) ClientPacketType.MatchReady, data) { }
    }
}
