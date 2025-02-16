using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class LogoutPacketHandler : ClientPacketHandler
    {
        public LogoutPacketHandler(byte[] data) 
            : base((int) ClientPacketType.Logout, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int info = reader.ReadInt32(); // 4 bytes in packet, not sure of the use
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;

            /* For some reason osu! sends a logout request when logging in. We'll ignore logout requests sent within a second after login. */
            if ((DateTime.Now - player.LoginTime).TotalSeconds < 1)
            {
                Console.WriteLine("Ignoring logout request as it has not been long enough since login.");
                return Task.CompletedTask;
            }

            // Log the player out
            bancho.RemovePlayer(player);
            return Task.CompletedTask;
        }
    }
}
