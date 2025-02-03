using OsuServer.API.Packets.Server;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class PingPacketHandler : ClientPacketHandler
    {
        public PingPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.Ping, data, osuToken, bancho) { }

        protected override void Handle(ref BinaryReader reader)
        {
            /* As far as I can tell, this packet is only sent to poll the server for pending packets.
             * We don't need to do anything here. */

            OnlinePlayer? player = Bancho.GetPlayer(Token);
            if (player == null) return;

            Console.WriteLine("Received a ping from " + player.Username);
        }
    }
}
