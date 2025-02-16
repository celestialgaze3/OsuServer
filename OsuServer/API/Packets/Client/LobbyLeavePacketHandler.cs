using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class LobbyLeavePacketHandler : ClientPacketHandler
    {
        public LobbyLeavePacketHandler(byte[] data) 
            : base((int) ClientPacketType.LobbyLeave, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;

            player.IsInLobby = false;
            Console.WriteLine($"Left the lobby");
            return Task.CompletedTask;
        }
    }
}
