using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class SelfStatsRequestPacketHandler : ClientPacketHandler
    {
        public SelfStatsRequestPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.SelfStatsRequest, data, osuToken, bancho) { }

        /// <summary>
        /// The client wants the stats of their own user
        /// </summary>
        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;

            // Send stats to self
            player.Connection.AddPendingPacket(new UserStatsPacket(player, player.Connection.Token, Bancho));

            Console.WriteLine("Received a self user stats request by " + player.Username);
            return Task.CompletedTask;
        }
    }
}
