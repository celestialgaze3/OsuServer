using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class FriendRemovePacketHandler : ClientPacketHandler
    {
        public FriendRemovePacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.FriendRemove, data, osuToken, bancho) { }

        protected override void Handle(ref BinaryReader reader)
        {
            int id = reader.ReadInt32();
            Player? player = Bancho.GetPlayer(Token);

            if (player == null) return;

            Player? toAdd = Bancho.GetPlayer(id);

            if (toAdd == null)
            {
                Console.WriteLine($"{player.Username} tried to remove non-existent player with ID {id} from their friends list.");
                return;
            }

            // Remove friend :(
            player.RemoveFriend(toAdd);
            Console.WriteLine($"{player.Username} removed {toAdd.Username} from their friends.");
        }
    }
}
