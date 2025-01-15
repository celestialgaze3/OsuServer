using OsuServer.State;

namespace OsuServer.API.Packets
{
    public abstract class ServerPacket : Packet
    {
        public ServerPacket(int id, string osuToken, Bancho bancho) : base(id, new byte[0], osuToken, bancho) { }

        public byte[] Encode()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    WriteData(binaryWriter);
                }

                return stream.ToArray();
            }
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
        ChannelJoinSuccess = 64,
        Channel = 65,
        ChannelKick = 66,
        ChannelAutoJoin = 67,
        Privileges = 71,
        ProtocolVersion = 75,
        UserPresence = 83,
        Reconnect = 86,
        EndChannelInfo = 89
    }
}
