using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchCompletePacket : ServerPacket
    {
        public MatchCompletePacket(Bancho bancho) 
            : base((int) ServerPacketType.MatchComplete) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // No need to write data
        }
    }
}
