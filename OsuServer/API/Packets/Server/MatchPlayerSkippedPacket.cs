using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchPlayerSkippedPacket : ServerPacket
    {

        private int _slotId;
        public MatchPlayerSkippedPacket(int slotId) 
            : base((int) ServerPacketType.MatchPlayerSkipped) 
        {
            _slotId = slotId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_slotId);
        }
    }

}
