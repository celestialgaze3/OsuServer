using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchLeavePacketHandler : ClientPacketHandler
    {
        public MatchLeavePacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchLeave, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;

            Match match = player.Match;
            player.LeaveMatch();
            match.BroadcastUpdate();
        }
    }
}
