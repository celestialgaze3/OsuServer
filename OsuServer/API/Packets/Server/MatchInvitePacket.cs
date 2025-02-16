using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchInvitePacket : ServerPacket
    {
        private OsuMessage _inviteMessage;
        public MatchInvitePacket(OsuMessage inviteMessage) 
            : base((int) ServerPacketType.MatchInvite) 
        {
            _inviteMessage = inviteMessage;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuMessage(_inviteMessage);
        }
    }
}
