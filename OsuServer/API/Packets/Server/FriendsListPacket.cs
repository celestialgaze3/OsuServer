using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class FriendsListPacket : ServerPacket
    {

        public HashSet<int> Friends { get; }
        public string OsuToken { get; }
        public FriendsListPacket(HashSet<int> friends, string osuToken, Bancho bancho) : base((int)ServerPacketType.FriendsList,
            osuToken, bancho)
        {
            Friends = friends;
            OsuToken = osuToken;
            Bancho = bancho;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteIntListShortLength(Friends);
        }
    }

}
