using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using static OsuServer.State.BanchoBeatmap;

namespace OsuServer.State
{
    public class BanchoBeatmap
    {
        private Bancho _bancho;
        public BeatmapExtended Info { get; private set; }


        private Dictionary<int, List<int>> _allScores = new();

        // Caches for the best of each stat (since we want to permanently track *all* non-failed scores)
        private Dictionary<int, int> _bestPPScores = new();
        private Dictionary<int, int> _bestScoreScores = new();
        private Dictionary<int, int> _bestComboScores = new();
        private Dictionary<int, int> _bestAccuracyScores = new();

        public BanchoBeatmap(Bancho bancho, BeatmapExtended beatmap)
        {
            _bancho = bancho;
            Info = beatmap;
        }
        
        /// <summary>
        /// Updates this beatmap with the given score
        /// </summary>
        /// <param name="player">The player that set the score</param>
        /// <param name="score">The score that was set</param>
        /// <returns>The old best score data for display on the submission screen</returns>
        public ScoreStats UpdateWithScore(Player player, SubmittedScore score)
        {
            if (score.Passed && !_allScores.ContainsKey(player.Id))
                _allScores.Add(player.Id, new List<int>());

            // Calculate old best stats from existing scores
            SubmittedScore? bestPPScore = null;
            if (_bestPPScores.ContainsKey(player.Id))
                bestPPScore = _bancho.Scores.GetById(_bestPPScores[player.Id]);

            SubmittedScore? bestScoreScore = null;
            if (_bestScoreScores.ContainsKey(player.Id))
                bestScoreScore = _bancho.Scores.GetById(_bestScoreScores[player.Id]);

            SubmittedScore? bestComboScore = null;
            if (_bestComboScores.ContainsKey(player.Id))
                bestComboScore = _bancho.Scores.GetById(_bestComboScores[player.Id]);

            SubmittedScore? bestAccuracyScore = null;
            if (_bestAccuracyScores.ContainsKey(player.Id))
                bestAccuracyScore = _bancho.Scores.GetById(_bestAccuracyScores[player.Id]);

            ScoreStats oldBestStats = new ScoreStats(bestPPScore, bestScoreScore, bestComboScore, bestAccuracyScore);

            // Overwrite existing best scores with this score if it is a better pass
            if (score.Passed)
            {
                if (bestPPScore == null || bestPPScore.PerformancePoints < score.PerformancePoints)
                    _bestPPScores[player.Id] = score.Id;

                if (bestScoreScore == null || bestScoreScore.TotalScore < score.TotalScore)
                    _bestScoreScores[player.Id] = score.Id;

                if (bestComboScore == null || bestComboScore.MaxCombo < score.MaxCombo)
                    _bestComboScores[player.Id] = score.Id;

                if (bestAccuracyScore == null || bestAccuracyScore.CalculateAccuracy() < score.CalculateAccuracy())
                    _bestAccuracyScores[player.Id] = score.Id;
            }

            return oldBestStats;
        }
    }
}
