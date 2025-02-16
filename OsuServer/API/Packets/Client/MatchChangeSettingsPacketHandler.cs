using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
using static OsuServer.State.Match;

namespace OsuServer.API.Packets.Client
{
    public class MatchChangeSettingsPacketHandler : ClientPacketHandler
    {
        public MatchChangeSettingsPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchChangeSettings, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            MatchData matchData = reader.ReadMatchData();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;

            Match match = player.Match;

            // Only hosts can change settings
            if (player.Id != match.HostId) return;

            // Update the map 
            if (matchData.BeatmapId == -1)
                match.MapBeingChanged();
            else if (matchData.BeatmapMD5 != match.BeatmapMD5)
            {
                BanchoBeatmap? beatmap = await bancho.GetBeatmap(database, matchData.BeatmapMD5);
                if (beatmap != null)
                {
                    // TODO: beatmapsets from database for last arg
                    match.SetBeatmap(beatmap.Info.Id, matchData.BeatmapMD5, matchData.BeatmapName);
                }
                else
                {
                    match.SetBeatmap(matchData.BeatmapId, matchData.BeatmapMD5, matchData.BeatmapName);
                }
            }

            // Update freemod status
            if (matchData.IsFreemod != match.IsFreemod)
            {
                if (matchData.IsFreemod)
                    match.EnableFreemod();
                else match.DisableFreemod();
            }

            // Update team mode
            MatchTeamMode teamMode = (MatchTeamMode)matchData.TeamMode;
            if (teamMode != match.TeamMode)
            {
                match.ChangeTeamMode(teamMode);
            }

            // Update other properties that don't require validation or other internal state updates
            match.WinCondition = (MatchWinCondition)matchData.WinCondition;
            match.Name = matchData.Name;

            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} updated settings for match ID {match.Id}");
        }
    }
}
