using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ToggleBlockMessagesPacketHandler : ClientPacketHandler
    {
        public ToggleBlockMessagesPacketHandler(byte[] data) 
            : base((int) ClientPacketType.ToggleBlockMessages, data) { }

        /// <summary>
        /// Client has toggled the in-game setting to block messages from non-friends
        /// </summary>
        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            bool blocked = reader.ReadInt32() != 0;
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;

            player.BlockingStrangerMessages = blocked;

            string blockedString = player.Username + " " + (blocked ? "is now " : "is no longer") + " blocking messages from non-friends.";
            Console.WriteLine(blockedString);
            return Task.CompletedTask;
        }
    }
}
