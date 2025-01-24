using OsuServer.Objects;

namespace OsuServer.State
{
    public class SubmittedScore : Score
    {
        public int Id { get; set; }
        public string Checksum { get; set; }

        private bool _wasPerformancePointsCalculated;
        private float _cachedPerformancePoints = 0;

        /// <summary>
        /// The amount of performance points awarded to this score. Will be calculated on demand if necessary, otherwise returns the cached value
        /// </summary>
        public float PerformancePoints
        {
            get
            {
                if (!_wasPerformancePointsCalculated)
                {
                    _cachedPerformancePoints = CalculatePerformancePoints();
                    _wasPerformancePointsCalculated = true;
                }

                return _cachedPerformancePoints;
            }

            private set
            {
                _cachedPerformancePoints = value;
            }
        }

        public SubmittedScore(Score score, int id, string checksum) : 
            base(score.Perfects, score.Goods, score.Bads, score.Gekis, score.Katus, 
                score.Misses, score.TotalScore, score.MaxCombo, score.PerfectCombo, 
                score.Grade, score.Mods, score.Passed, score.GameMode)
        {
            Id = id;
            Checksum = checksum;
        }

        private float CalculatePerformancePoints()
        {
            // TODO: Actual performance points calculations
            return Perfects; // placeholder
        }

        /// <summary>
        /// Updates this score's cached performance points value
        /// </summary>
        public void RecalculatePerformancePoints()
        {
            PerformancePoints = CalculatePerformancePoints();
        }
    }
}
