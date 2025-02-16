using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class LobbyJoinPacketHandler : ClientPacketHandler
    {
        public LobbyJoinPacketHandler(byte[] data) 
            : base((int) ClientPacketType.LobbyJoin, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;
            
            // Mark the player as in the lobby (multiplayer lobby selection screen)
            player.IsInLobby = true;

            // Send a copy of each active multiplayer match to the client
            foreach (var matchEntry in bancho.Matches.All)
            {
                MatchCreatePacket matchPacket = new(matchEntry.Value);
                player.Connection.AddPendingPacket(matchPacket);
            }

            Console.WriteLine($"Returned {bancho.Matches.All.Count} matches");
            return Task.CompletedTask;
        }
    }
}
