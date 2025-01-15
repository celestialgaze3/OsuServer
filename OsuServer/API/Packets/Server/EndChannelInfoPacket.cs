using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class EndChannelInfoPacket : ServerPacket
    {
        public EndChannelInfoPacket(string osuToken, Bancho bancho) : base((int) ServerPacketType.EndChannelInfo, osuToken, bancho) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // Empty body
        }
    }
}
