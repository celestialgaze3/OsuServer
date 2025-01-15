using OsuServer.State;
using OsuServer.Util;
using System.Text;

namespace OsuServer.API.Packets.Server
{
    public class NotificationPacket : ServerPacket
    {
        string Message;
        public NotificationPacket(string message, string osuToken, Bancho bancho) : base((int) ServerPacketType.Notification, osuToken, bancho) 
        {
            Message = message;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuString(Message);
        }
    }
}
