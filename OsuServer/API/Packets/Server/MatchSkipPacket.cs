using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchSkipPacket : ServerPacket
    {
        public MatchSkipPacket(Bancho bancho) 
            : base((int) ServerPacketType.MatchSkip) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // No data needs to be written
        }
    }
}
