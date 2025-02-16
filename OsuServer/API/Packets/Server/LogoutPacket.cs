using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class LogoutPacket : ServerPacket
    {
        int UserId;
        public LogoutPacket(int userId) 
            : base((int) ServerPacketType.Logout) 
        {
            UserId = userId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(UserId);
        }
    }
}
