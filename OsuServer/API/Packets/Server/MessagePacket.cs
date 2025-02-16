using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MessagePacket : ServerPacket
    {
        OsuMessage Message;
        public MessagePacket(OsuMessage message) 
            : base((int) ServerPacketType.Message) 
        {
            Message = message;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuMessage(Message);
        }
    }
}
