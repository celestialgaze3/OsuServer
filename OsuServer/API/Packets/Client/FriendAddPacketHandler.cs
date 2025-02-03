using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class FriendAddPacketHandler : ClientPacketHandler
    {
        public FriendAddPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.FriendAdd, data, osuToken, bancho) { }

        protected override void Handle(ref BinaryReader reader)
        {
            int id = reader.ReadInt32();
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return;

            OnlinePlayer? toAdd = Bancho.GetPlayer(id);

            if (toAdd == null)
            {
                Console.WriteLine($"{player.Username} tried to add non-existent player with ID {id} to their friends list.");
                return;
            }

            // Add friend :D
            player.AddFriend(toAdd);
            Console.WriteLine($"{player.Username} added {toAdd.Username} as a friend!");
        }
    }
}
