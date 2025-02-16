using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchCreatePacket : ServerPacket
    {
        Match _match;
        public MatchCreatePacket(Match match) 
            : base((int) ServerPacketType.MatchCreate) 
        {
            _match = match;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteMatchData(_match.Data, false);
        }
    }
}
