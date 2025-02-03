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
        private OnlinePlayer _player;
        private ProfileStats _oldStats;
        private ProfileStats _newStats;
        private ScoreStats _oldScore;
        public ScoreStats _newScore;
        public ScoreReport(Bancho bancho, BeatmapExtended beatmap, OnlinePlayer player, ProfileStats oldStats, ProfileStats newStats, ScoreStats oldScore, ScoreStats newScore)
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
                // TODO: map rankings
                UpdatedValue("rank", 0, 0),
                UpdatedValue("rankedScore", _oldScore.TotalScore, _newScore.TotalScore),
                UpdatedValue("totalScore", _oldScore.TotalScore, _newScore.TotalScore),
                UpdatedValue("maxCombo", _oldScore.Combo, _newScore.Combo),
                UpdatedValue("accuracy", (float)_oldScore.Accuracy * 100f, (float)_newScore.Accuracy * 100f),
                UpdatedValue("pp", (float)Math.Round(_oldScore.PerformancePoints), (float)Math.Round(_newScore.PerformancePoints)),
                $"onlineScoreId:{_bancho.Scores.GetByChecksum(scoreChecksum)}",
                "\n",
                $"chartId:overall",
                $"chartUrl:https://{ServerConfiguration.Domain}/u/{_player.Id}",
                $"chartName:Overall Ranking",
                UpdatedValue("rank", _oldStats.Rank, _newStats.Rank),
                UpdatedValue("rankedScore", _oldStats.RankedScore, _newStats.RankedScore),
                UpdatedValue("totalScore", _oldStats.TotalScore, _newStats.TotalScore),
                UpdatedValue("maxCombo", _oldStats.MaxCombo, _newStats.MaxCombo),
                UpdatedValue("accuracy", (float)_oldStats.Accuracy * 100f, (float)_newStats.Accuracy * 100f),
                UpdatedValue("pp", (float)Math.Round(_oldStats.PP), (float)Math.Round(_newStats.PP)),
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
