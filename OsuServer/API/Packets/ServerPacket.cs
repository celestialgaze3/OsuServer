using OsuServer.State;

namespace OsuServer.API.Packets
{
    public abstract class ServerPacket : Packet
    {
        public ServerPacket(int id) : base(id, new byte[0]) { }

        public byte[] Encode()
        {
            using MemoryStream stream = new();
            using (BinaryWriter binaryWriter = new(stream))
            {
                WriteData(binaryWriter);
            }

            return stream.ToArray();
        }

        public override byte[] GetBytes()
        {
            Data = Encode();
            return base.GetBytes();
        }

        protected abstract void WriteData(BinaryWriter binaryWriter);
    }

    enum ServerPacketType
    {
        Login = 5,
        Message = 7,
        Pong = 8,
        UserStats = 11,
        Logout = 12,
        Notification = 24,
        MatchUpdate = 26,
        MatchCreate = 27,
        MatchRemove = 28,
        MatchJoinSuccess = 36,
        MatchJoinFail = 37,
        MatchStart = 46,
        MatchScoreUpdate = 48,
        MatchPlayersLoaded = 53,
        MatchFailed = 57,
        MatchComplete = 58,
        MatchSkip = 61,
        ChannelJoinSuccess = 64,
        Channel = 65,
        ChannelKick = 66,
        ChannelAutoJoin = 67,
        Privileges = 71,
        FriendsList = 72,
        ProtocolVersion = 75,
        MatchPlayerSkipped = 81,
        UserPresence = 83,
        Reconnect = 86,
        MatchInvite = 88,
        EndChannelInfo = 89,
        MatchAbort = 106
    }
}
