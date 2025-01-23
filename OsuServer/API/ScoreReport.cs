using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using OsuServer.State;
using System.Runtime.CompilerServices;

namespace OsuServer.API
{
    public class ScoreReport
    {
        private Bancho _bancho;
        private BeatmapExtended _beatmap;
        private Player _player;
        private PlayerStats _oldStats;
        private PlayerStats _newStats;
        private Score? _oldScore;
        public Score _newScore;
        public ScoreReport(Bancho bancho, BeatmapExtended beatmap, Player player, PlayerStats oldStats, PlayerStats newStats, Score? oldScore, Score newScore)
        {
            _bancho = bancho;
            _oldScore = oldScore;
            _newScore = newScore;
            _oldStats = oldStats;
            _newStats = newStats;
            _player = player;
            _beatmap = beatmap;
        }

        // TODO: Score checksum calculating to remove this argument
        public string GenerateString(string scoreChecksum)
        {
            if (_beatmap.BeatmapSet == null)
            {
                Console.WriteLine($"Beatmap set was null for beatmap ID {_beatmap.Id}");
            }
            string[] report = [
                $"beatmapId:{_beatmap.Id}",
                $"beatmapSetId:{(_beatmap.BeatmapSet != null ? _beatmap.BeatmapSet.Id : 0)}",
                $"beatmapPlaycount:{_beatmap.Playcount}",
                $"beatmapPasscount:{_beatmap.Passcount}",
                $"approvedDate:{_beatmap.LastUpdated.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'-'zzz")}",
                "\n",
                $"chartId:beatmap",
                $"chartUrl:https://{ServerConfiguration.Domain}/b/{_beatmap.Id}",
                $"chartName:Beatmap Ranking",
                // TODO: these values are the updated map rank statistics. implement when you can track that
                UpdatedValue("rank", 0, 0),
                UpdatedValue("rankedScore", 0, 0),
                UpdatedValue("totalScore", 0, 0),
                UpdatedValue("maxCombo", 0, 0),
                UpdatedValue("accuracy", 0, 0),
                UpdatedValue("pp", 0, 0),
                $"onlineScoreId:{_bancho.GetScoreId(scoreChecksum)}",
                "\n",
                $"chartId:overall",
                $"chartUrl:https://{ServerConfiguration.Domain}/u/{_player.Id}",
                $"chartName:Overall Ranking",
                UpdatedValue("rank", _oldStats.Rank, _newStats.Rank),
                UpdatedValue("rankedScore", _oldStats.RankedScore, _newStats.RankedScore),
                UpdatedValue("totalScore", _oldStats.TotalScore, _newStats.TotalScore),
                UpdatedValue("maxCombo", _oldStats.MaxCombo, _newStats.MaxCombo),
                UpdatedValue("accuracy", _oldStats.Accuracy * 100f, _newStats.Accuracy * 100f),
                UpdatedValue("pp", _oldStats.PP, _newStats.PP),
                $"achievements-new:" // TODO: achievements
            ];
            return string.Join("|", report);
        }

        private string UpdatedValue(string field, float? oldValue, float? newValue)
        {
            return $"{field}Before:{(oldValue != null ? oldValue : "")}|{field}After:{(newValue != null ? newValue : "")}";
        }

    }
}
