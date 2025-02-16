using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class FriendAddPacketHandler : ClientPacketHandler
    {
        public FriendAddPacketHandler(byte[] data) 
            : base((int) ClientPacketType.FriendAdd, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int id = reader.ReadInt32();
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return;

            OnlinePlayer? toAdd = bancho.GetPlayer(id);

            if (toAdd == null)
            {
                Console.WriteLine($"{player.Username} tried to add non-existent player with ID {id} to their friends list.");
                return;
            }

            // Add friend :D
            await player.AddFriend(database, toAdd);
            Console.WriteLine($"{player.Username} added {toAdd.Username} as a friend!");
        }
    }
}
