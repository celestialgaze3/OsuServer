using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ProtocolVersionPacket : ServerPacket
    {
        int ProtocolVersion;
        public ProtocolVersionPacket(int protocolVersion, string osuToken, Bancho bancho) : base((int) ServerPacketType.ProtocolVersion, osuToken, bancho)
        {
            ProtocolVersion = protocolVersion;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ProtocolVersion);
        }
    }
}
