using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class PingPacketHandler : ClientPacketHandler
    {
        public PingPacketHandler(byte[] data) 
            : base((int) ClientPacketType.Ping, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            /* As far as I can tell, this packet is only sent to poll the server for pending packets.
             * We don't need to do anything here. */

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;

            Console.WriteLine("Received a ping from " + player.Username);
            return Task.CompletedTask;
        }
    }
}
