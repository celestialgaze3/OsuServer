using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchJoinFailPacket : ServerPacket
    {
        public MatchJoinFailPacket(Bancho bancho) 
            : base((int) ServerPacketType.MatchJoinFail) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // No data needs to be written
        }
    }
}
