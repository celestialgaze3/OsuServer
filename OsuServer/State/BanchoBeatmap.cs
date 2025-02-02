using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using static OsuServer.State.BanchoBeatmap;

namespace OsuServer.State
{
    public class BanchoBeatmap
    {
        private Bancho _bancho;
        public BeatmapExtended Info { get; private set; }

        private Dictionary<int, List<int>> _allScores = [];
        private Dictionary<Score, double> _ppCache = [];

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
            if (!_allScores.ContainsKey(player.Id))
                _allScores.Add(player.Id, new List<int>());

            // Calculate old best stats from existing scores
            ScoreStats oldBestStats = new ScoreStats(_bancho, _allScores[player.Id]);
            return oldBestStats;
        }

        public double CalculatePerformancePoints(Score score)
        {
            double pp;
            if (_ppCache.TryGetValue(score, out pp))
            {
                return pp;
            }

            // TODO: proper performance points calculation
            pp = score.Perfects;

            return pp;
        }
    }
}
