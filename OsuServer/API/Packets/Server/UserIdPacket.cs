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

    public enum LoginFailureType
    {
        AuthenticationFailed = -1,
        ClientTooOld = -2,
        Banned = -3,
        BannedButDifferent = -4,
        Error = -5,
        NeedsSupporter = -6,
        PasswordReset = -7,
        NeedVerification = -8
    }
}
