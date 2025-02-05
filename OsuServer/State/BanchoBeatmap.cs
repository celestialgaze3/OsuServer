﻿using OsuServer.External.OsuV2Api;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class BanchoBeatmap
    {
        private Bancho _bancho;
        public BeatmapExtended Info { get; private set; }

        private Dictionary<Score, double> _ppCache = [];

        public BanchoBeatmap(Bancho bancho, BeatmapExtended beatmap)
        {
            _bancho = bancho;
            Info = beatmap;
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
                Info.Ranked = RankStatus.Ranked;

            return Info.Ranked;
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
