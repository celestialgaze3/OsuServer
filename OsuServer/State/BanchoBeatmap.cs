using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using System.Numerics;
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
        public ScoreStats UpdateWithScore(OnlinePlayer player, SubmittedScore score)
        {
            if (!_allScores.ContainsKey(player.Id))
                _allScores.Add(player.Id, new List<int>());

            // Calculate old best stats from existing scores
            ScoreStats oldBestStats = new ScoreStats(_bancho, _allScores[player.Id]);

            // Add this score to the beatmap score listing
            _allScores[player.Id].Add(score.Id);

            return oldBestStats;
        }

        /// <summary>
        /// Adds a score to this beatmap without affecting state
        /// </summary>
        /// <param name="player">The player that set the score</param>
        /// <param name="score">The score to add</param>
        public void AddScore(OnlinePlayer player, SubmittedScore score)
        {
            if (!_allScores.ContainsKey(player.Id))
                _allScores.Add(player.Id, new List<int>());

            if (!_allScores[player.Id].Contains(score.Id))
                _allScores[player.Id].Add(score.Id);
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

        /// <returns>This beatmap's adjusted ranked status</returns>
        public RankStatus GetRankStatus()
        {
            // hehe TODO: custom ranking
            if (Info.UserId == 10321695)
                Info.RankStatus = RankStatus.Ranked;

            return Info.RankStatus;
        }

        public bool ShouldAwardStatIncrease()
        {
            RankStatus status = GetRankStatus();
            if (status == RankStatus.Ranked || status == RankStatus.Approved ||
                status == RankStatus.Loved)
            {
                return true;
            }

            // Graveyard, pending, qualified
            return false;
        }
    }

}
