using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class PongPacket : ServerPacket
    {
        public PongPacket(Bancho bancho) 
            : base((int) ServerPacketType.Pong) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // Empty body
        }
    }
}
