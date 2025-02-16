using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchStartPacket : ServerPacket
    {
        private Match _match;
        public MatchStartPacket(Match match) 
            : base((int) ServerPacketType.MatchStart) 
        {
            _match = match;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteMatchData(_match.Data, true);
        }
    }
}
