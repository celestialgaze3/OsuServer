using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ProtocolVersionPacket : ServerPacket
    {
        int ProtocolVersion;
        public ProtocolVersionPacket(int protocolVersion) 
            : base((int) ServerPacketType.ProtocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ProtocolVersion);
        }
    }
}
