using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class EndChannelInfoPacket : ServerPacket
    {
        public EndChannelInfoPacket() 
            : base((int) ServerPacketType.EndChannelInfo) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // Empty body
        }
    }
}
