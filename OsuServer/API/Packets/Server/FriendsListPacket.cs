using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class FriendsListPacket : ServerPacket
    {
        public HashSet<int> Friends { get; }
        public FriendsListPacket(HashSet<int> friends) 
            : base((int)ServerPacketType.FriendsList)
        {
            Friends = friends;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteIntListShortLength(Friends);
        }
    }

}
