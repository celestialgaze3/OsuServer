using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class LogoutPacket : ServerPacket
    {
        int UserId;
        public LogoutPacket(int userId, string osuToken, Bancho bancho) : base((int) ServerPacketType.Logout, osuToken, bancho) 
        {
            UserId = userId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(UserId);
        }
    }
}
