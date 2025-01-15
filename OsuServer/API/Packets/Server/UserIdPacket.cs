using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class UserIdPacket : ServerPacket
    {
        int UserId;
        public UserIdPacket(int userId, string osuToken, Bancho bancho) : base((int) ServerPacketType.Login, osuToken, bancho) 
        {
            UserId = userId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(UserId);
        }
    }
}
