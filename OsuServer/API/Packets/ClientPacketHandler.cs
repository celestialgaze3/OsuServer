using OsuServer.API.Packets.Client;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets
{
    public abstract class ClientPacketHandler : Packet
    {
        private static Dictionary<int, Func<byte[], ClientPacketHandler>> s_PacketHandlers = [];
        protected ClientPacketHandler(int id, byte[] data) : base(id, data) { }

        private static void RegisterPacketType(int id, Func<byte[], ClientPacketHandler> factory)
        {
            s_PacketHandlers.Add(id, factory);
        }

        private static ClientPacketHandler GetHandlerFor(int id, byte[] data)
        {
            if (s_PacketHandlers.TryGetValue(id, out var handlerFactory))
            {
                return handlerFactory.Invoke(data);
            }
            else
            {
                return new UnknownPacketHandler(id, data);
            }
        }

        public static void RegisterPacketTypes()
        {
            RegisterPacketType((int) ClientPacketType.Ping,               (byte[] bytes) => 
                new PingPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.ChannelJoin,        (byte[] bytes) =>
                new ChannelJoinPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.UserStats,          (byte[] bytes) => 
                new UsersStatsRequestPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.MessageChannel,     (byte[] bytes) =>
                new MessageChannelPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.PresenceFilter,     (byte[] bytes) =>
               new PresenceFilterPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.ChannelLeave,       (byte[] bytes) =>
               new ChannelLeavePacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.Logout,             (byte[] bytes) =>
               new LogoutPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.UserUpdate,         (byte[] bytes) =>
               new UserUpdatePacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.MessageUser,        (byte[] bytes) =>
               new MessageUserPacketHandler(bytes));

            RegisterPacketType((int) ClientPacketType.SelfStats,          (byte[] bytes) =>
               new SelfStatsRequestPacketHandler(bytes));

            RegisterPacketType((int)ClientPacketType.ToggleBlockMessages, (byte[] bytes) =>
               new ToggleBlockMessagesPacketHandler(bytes));

            // Friends
            RegisterPacketType((int)ClientPacketType.FriendAdd,           (byte[] bytes) =>
               new FriendAddPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.FriendRemove,        (byte[] bytes) =>
               new FriendRemovePacketHandler(bytes));

            // Multiplayer
            RegisterPacketType((int)ClientPacketType.MatchCreate,         (byte[] bytes) =>
               new MatchCreatePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.LobbyJoin,           (byte[] bytes) =>
               new LobbyJoinPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.LobbyLeave,          (byte[] bytes) =>
               new LobbyLeavePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchChangeSettings, (byte[] bytes) =>
               new MatchChangeSettingsPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchJoin,           (byte[] bytes) =>
               new MatchJoinPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchLeave,          (byte[] bytes) =>
               new MatchLeavePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchSlotChange,     (byte[] bytes) =>
               new MatchSlotChangePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchReady,          (byte[] bytes) =>
               new MatchReadyPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchNotReady,       (byte[] bytes) =>
               new MatchNotReadyPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchSlotToggleLock, (byte[] bytes) =>
               new MatchSlotToggleLockPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchChangePassword, (byte[] bytes) =>
               new MatchChangePasswordPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchChangeTeam,     (byte[] bytes) =>
               new MatchChangeTeamPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchNoBeatmap,      (byte[] bytes) =>
               new MatchNoBeatmapPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchHasBeatmap,     (byte[] bytes) =>
               new MatchHasBeatmapPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchStart,          (byte[] bytes) =>
               new MatchStartPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchChangeMods,     (byte[] bytes) =>
               new MatchChangeModsPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchPlayerLoaded,   (byte[] bytes) =>
               new MatchPlayerLoadedPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchScoreUpdate,    (byte[] bytes) =>
               new MatchScoreUpdatePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchFailed,         (byte[] bytes) =>
               new MatchFailedPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchPlayerComplete, (byte[] bytes) =>
               new MatchPlayerCompletePacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchPlayerSkip,     (byte[] bytes) =>
               new MatchPlayerSkipPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchChangeHost,     (byte[] bytes) =>
               new MatchChangeHostPacketHandler(bytes));
            RegisterPacketType((int)ClientPacketType.MatchInvite, (byte[] bytes) =>
               new MatchInvitePacketHandler(bytes));

        }

        public async Task Handle(OsuServerDb database, Bancho bancho, string osuToken)
        {
            var stream = new MemoryStream(Data);
            var reader = new BinaryReader(stream);

            await Handle(database, bancho, osuToken, reader);

            reader.Dispose();
            stream.Dispose();
        }

        /// <summary>
        /// Update the state of Bancho based on the data within this packet
        /// </summary>
        /// <param name="reader">A BinaryReader loaded with the data contained within this packet</param>
        protected abstract Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader);

        private static ClientPacketHandler ParseIncomingPacket(MemoryStream stream, BinaryReader binaryReader)
        {
            int id;
            byte[] data;

            id = binaryReader.ReadUInt16();
            binaryReader.ReadByte(); // Padding byte
            int length = binaryReader.ReadInt32();
            data = binaryReader.ReadBytes(length);
            Console.WriteLine($"Parsed packet {(ClientPacketType)id} with data {BitConverter.ToString(data)}");

            return GetHandlerFor(id, data);
        }

        public static List<ClientPacketHandler> ParseIncomingPackets(MemoryStream stream)
        {
            List<ClientPacketHandler> packets = [];
            using (BinaryReader binaryReader = new(stream))
            {
                while (binaryReader.PeekChar() != -1)
                {
                    packets.Add(ParseIncomingPacket(stream, binaryReader));
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
        SelfStats = 3,
        Ping = 4,
        MessageUser = 25,
        LobbyLeave = 29,
        LobbyJoin = 30,
        MatchCreate = 31,
        MatchJoin = 32,
        MatchLeave = 33,
        MatchSlotChange = 38,
        MatchReady = 39,
        MatchSlotToggleLock = 40,
        MatchChangeSettings = 41,
        MatchStart = 44,
        MatchScoreUpdate = 47,
        MatchPlayerComplete = 49,
        MatchChangeMods = 51,
        MatchPlayerLoaded = 52,
        MatchNoBeatmap = 54,
        MatchNotReady = 55,
        MatchFailed = 56,
        MatchHasBeatmap = 59,
        MatchPlayerSkip = 60,
        ChannelJoin = 63,
        MatchChangeHost = 70,
        FriendAdd = 73,
        FriendRemove = 74,
        MatchChangeTeam = 77,
        ChannelLeave = 78,
        PresenceFilter = 79,
        UserStats = 85,
        MatchInvite = 87,
        MatchChangePassword = 90,
        ToggleBlockMessages = 99
    }
}
