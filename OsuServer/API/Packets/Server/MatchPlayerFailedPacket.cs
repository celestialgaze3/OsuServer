using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchPlayerFailedPacket : ServerPacket
    {
        byte _slotId;
        public MatchPlayerFailedPacket(byte slotId) 
            : base((int) ServerPacketType.MatchFailed) 
        { 
            _slotId = slotId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_slotId);
        }
    }
}
