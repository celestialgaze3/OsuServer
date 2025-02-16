using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class SelfStatsRequestPacketHandler : ClientPacketHandler
    {
        public SelfStatsRequestPacketHandler(byte[] data) 
            : base((int) ClientPacketType.SelfStats, data) { }

        /// <summary>
        /// The client wants the stats of their own user
        /// </summary>
        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;

            // Send stats to self
            player.Connection.AddPendingPacket(new UserStatsPacket(player));

            Console.WriteLine("Received a self user stats request by " + player.Username);
            return Task.CompletedTask;
        }
    }
}
