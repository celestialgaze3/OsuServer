using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class PongPacket : ServerPacket
    {
        public PongPacket(string osuToken, Bancho bancho) : base((int) ServerPacketType.Pong, osuToken, bancho) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // Empty body
        }
    }
}
