using OsuServer.API.Packets.Client;
using OsuServer.State;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OsuServer.API.Packets
{
    public abstract class ClientPacketHandler : Packet
    {
        private static Dictionary<int, Func<byte[], string, Bancho, ClientPacketHandler>> s_PacketHandlers = new Dictionary<int, Func<byte[], string, Bancho, ClientPacketHandler>>();
        protected ClientPacketHandler(int id, byte[] data, string osuToken, Bancho bancho) : base(id, data, osuToken, bancho) { }

        private static void RegisterPacketType(int id, Func<byte[], string, Bancho, ClientPacketHandler> factory)
        {
            s_PacketHandlers.Add(id, factory);
        }

        private static ClientPacketHandler GetHandlerFor(int id, byte[] data, string osuToken, Bancho bancho)
        {
            if (s_PacketHandlers.TryGetValue(id, out var handlerFactory))
            {
                return handlerFactory.Invoke(data, osuToken, bancho);
            }
            else
            {
                return new UnknownPacketHandler(id, data, osuToken, bancho);
            }
        }

        public static void RegisterPacketTypes()
        {
            RegisterPacketType((int) ClientPacketType.Ping,               (byte[] bytes, string osuToken, Bancho bancho) => 
                new PingPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.ChannelJoin,        (byte[] bytes, string osuToken, Bancho bancho) =>
                new ChannelJoinPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.UserStatsRequest,   (byte[] bytes, string osuToken, Bancho bancho) => 
                new UsersStatsRequestPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.MessageChannel,     (byte[] bytes, string osuToken, Bancho bancho) =>
                new MessageChannelPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.PresenceFilter,     (byte[] bytes, string osuToken, Bancho bancho) =>
               new PresenceFilterPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.ChannelLeave,       (byte[] bytes, string osuToken, Bancho bancho) =>
               new ChannelLeavePacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.Logout,             (byte[] bytes, string osuToken, Bancho bancho) =>
               new LogoutPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.UserUpdate,         (byte[] bytes, string osuToken, Bancho bancho) =>
               new UserUpdatePacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.MessageUser,        (byte[] bytes, string osuToken, Bancho bancho) =>
               new MessageUserPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int) ClientPacketType.SelfStatsRequest,   (byte[] bytes, string osuToken, Bancho bancho) =>
               new SelfStatsRequestPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int)ClientPacketType.ToggleBlockMessages, (byte[] bytes, string osuToken, Bancho bancho) =>
               new ToggleBlockMessagesPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int)ClientPacketType.FriendAdd, (byte[] bytes, string osuToken, Bancho bancho) =>
               new FriendAddPacketHandler(bytes, osuToken, bancho));

            RegisterPacketType((int)ClientPacketType.FriendRemove, (byte[] bytes, string osuToken, Bancho bancho) =>
               new FriendRemovePacketHandler(bytes, osuToken, bancho));
        }

        
        public void Handle()
        {
            var stream = new MemoryStream(Data);
            var reader = new BinaryReader(stream);

            Handle(ref reader);

            reader.Dispose();
            stream.Dispose();
        }

        /// <summary>
        /// Update the state of Bancho based on the data within this packet
        /// </summary>
        /// <param name="reader">A BinaryReader loaded with the data contained within this packet</param>
        protected abstract void Handle(ref BinaryReader reader);

        private static ClientPacketHandler ParseIncomingPacket(MemoryStream stream, string osuToken, Bancho bancho, BinaryReader binaryReader)
        {
            int id;
            byte[] data;

            id = binaryReader.ReadUInt16();
            binaryReader.ReadByte(); // Padding byte
            int length = binaryReader.ReadInt32();
            data = binaryReader.ReadBytes(length);
            Console.WriteLine("Parsed packet ID " + id + " with data " + BitConverter.ToString(data) + " from " + osuToken);

            return GetHandlerFor(id, data, osuToken, bancho);
        }

        public static List<ClientPacketHandler> ParseIncomingPackets(MemoryStream stream, string osuToken, Bancho bancho)
        {
            List<ClientPacketHandler> packets = new List<ClientPacketHandler>();
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                while (binaryReader.PeekChar() != -1)
                {
                    packets.Add(ParseIncomingPacket(stream, osuToken, bancho, binaryReader));
                }
            }
            Console.WriteLine("Parsed " + packets.Count + " packets from request");
            return packets;
        }
    }

    enum ClientPacketType 
    {
        UserUpdate = 0,
        MessageChannel = 1,
        Logout = 2,
        SelfStatsRequest = 3,
        Ping = 4,
        MessageUser = 25,
        ChannelJoin = 63,
        FriendAdd = 73,
        FriendRemove = 74,
        ChannelLeave = 78,
        PresenceFilter = 79,
        UserStatsRequest = 85,
        ToggleBlockMessages = 99
    }
}
