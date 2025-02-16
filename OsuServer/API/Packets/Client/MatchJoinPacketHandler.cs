using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchJoinPacketHandler : ClientPacketHandler
    {
        public MatchJoinPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchJoin, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int id = reader.ReadInt32();
            string password = reader.ReadOsuString();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (player.IsInLobby)
            {
                player.LeaveMatch();
            }

            Match? match;
            if (bancho.Matches.TryGetValue(id, out match))
            {
                if (player.TryJoinMatch(match, password))
                {
                    match.BroadcastUpdate();
                }
            } else
            {
                player.Connection.AddPendingPacket(new MatchJoinFailPacket(bancho));
            }
        }
    }
}
