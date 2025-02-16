using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchNotReadyPacketHandler : MatchChangeSlotStatusPacketHandler
    {
        public MatchNotReadyPacketHandler(byte[] data) 
            : base(MatchSlot.SlotStatus.NotReady, (int) ClientPacketType.MatchNotReady, data) { }

    }
}
