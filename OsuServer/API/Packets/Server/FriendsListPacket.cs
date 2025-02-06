using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class FriendsListPacket : ServerPacket
    {

        public List<int> Friends { get; private set; } 
        public FriendsListPacket(List<int> friends, string osuToken, Bancho bancho) : base((int) ServerPacketType.FriendsList,
            osuToken, bancho) 
        {
            Friends = friends;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteIntListShortLength(Friends);
        }
    }

}
