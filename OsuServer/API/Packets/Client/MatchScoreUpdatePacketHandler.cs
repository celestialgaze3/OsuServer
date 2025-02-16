using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchScoreUpdatePacketHandler : ClientPacketHandler
    {
        public MatchScoreUpdatePacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchScoreUpdate, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            LiveScoreData data = reader.ReadLiveScoreData();
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Send score data to fellow players
            match.BroadcastLiveScoreData(player, data);

            Console.WriteLine($"{player.Username} updated their score data in match ID {match.Id}!");
        }
    }
}
