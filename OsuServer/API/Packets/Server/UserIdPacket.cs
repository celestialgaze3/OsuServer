using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class UserIdPacket : ServerPacket
    {

        private int _userId;
        public UserIdPacket(int userId) 
            : base((int) ServerPacketType.Login) 
        {
            _userId = userId;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_userId);
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
