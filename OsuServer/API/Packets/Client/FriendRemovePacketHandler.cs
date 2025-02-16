using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class FriendRemovePacketHandler : ClientPacketHandler
    {
        public FriendRemovePacketHandler(byte[] data) 
            : base((int) ClientPacketType.FriendRemove, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int id = reader.ReadInt32();
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return;

            OnlinePlayer? toAdd = bancho.GetPlayer(id);

            if (toAdd == null)
            {
                Console.WriteLine($"{player.Username} tried to remove non-existent player with ID {id} from their friends list.");
                return;
            }

            // Remove friend :(
            await player.RemoveFriend(database, toAdd);
            Console.WriteLine($"{player.Username} removed {toAdd.Username} from their friends.");
        }
    }
}
