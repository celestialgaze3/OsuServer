using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchRemovePacket : ServerPacket
    {
        private int _matchId;
        public MatchRemovePacket(int matchId) 
            : base((int) ServerPacketType.MatchRemove) 
        {
            _matchId = matchId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_matchId);
        }
    }
}
