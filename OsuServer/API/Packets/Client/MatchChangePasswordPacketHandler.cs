using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchChangePasswordPacketHandler : ClientPacketHandler
    {
        public MatchChangePasswordPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchChangePassword, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            MatchData matchData = reader.ReadMatchData();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;

            Match match = player.Match;

            // Only hosts can change settings
            if (player.Id != match.HostId) return;

            match.Password = matchData.Password;
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} updated settings for match ID {match.Id}");
        }
    }
}
