using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchJoinSuccessPacket : ServerPacket
    {
        private Match _match;
        public MatchJoinSuccessPacket(Match match) 
            : base((int) ServerPacketType.MatchJoinSuccess) 
        {
            _match = match;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteMatchData(_match.Data, true);
        }
    }
}
