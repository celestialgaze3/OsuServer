using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchUpdatePacket : ServerPacket
    {
        private Match _match;
        private bool _sendPassword;
        public MatchUpdatePacket(Match match, bool sendPassword) 
            : base((int) ServerPacketType.MatchUpdate) 
        {
            _match = match;
            _sendPassword = sendPassword;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteMatchData(_match.Data, _sendPassword);
        }
    }
}
