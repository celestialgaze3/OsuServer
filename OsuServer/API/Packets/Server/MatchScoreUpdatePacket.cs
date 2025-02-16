using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchScoreUpdatePacket : ServerPacket
    {
        LiveScoreData _matchScoreData;
        public MatchScoreUpdatePacket(LiveScoreData match) 
            : base((int) ServerPacketType.MatchScoreUpdate) 
        {
            _matchScoreData = match;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteLiveScoreData(_matchScoreData);
        }
    }
}
