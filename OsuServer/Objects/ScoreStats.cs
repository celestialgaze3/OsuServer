using OsuServer.State;

namespace OsuServer.Objects
{
    public class ScoreStats
    {
        public float PerformancePoints { get; set; } = 0.0f;
        public int TotalScore { get; set; } = 0;
        public int Combo { get; set; } = 0;
        public float Accuracy { get; set; } = 0.0f;

        public ScoreStats(int performancePoints, int score, int combo, float accuracy)
        {
            PerformancePoints = performancePoints;
            TotalScore = score;
            Combo = combo;
            Accuracy = accuracy;
        }

        /// <summary>
        /// Fills this ScoreStats instance with the stats of the given score
        /// </summary>
        /// <param name="score">The score to copy stats from</param>
        public ScoreStats(SubmittedScore score)
        {
            PerformancePoints = score.PerformancePoints;
            TotalScore = score.TotalScore;
            Combo = score.MaxCombo;
            Accuracy = score.CalculateAccuracy();
        }

        /// <summary>
        /// Fills this ScoreStats instance with the highest stats from the given scores
        /// </summary>
        /// <param name="scores">List of scores</param>
        public ScoreStats(params SubmittedScore?[] scores)
        {
            for (int i = 0; i < scores.Length; i++)
            {
                SetBestValues(scores[i]);
            }
        }

        /// <summary>
        /// Fills this ScoreStats instance with the highest stats from the given scores
        /// </summary>
        /// <param name="bancho">The bancho instance containing thse score IDs</param>
        /// <param name="scoreIds">List of score IDs</param>
        public ScoreStats(Bancho bancho, List<int> scoreIds)
        {
            for (int i = 0; i < scoreIds.Count; i++)
            {
                SubmittedScore? score = bancho.Scores.GetById(scoreIds[i]);
                SetBestValues(score);
            }
        }

        private void SetBestValues(SubmittedScore? score)
        {
            if (score == null) return;

            if (PerformancePoints < score.PerformancePoints)
            {
                PerformancePoints = score.PerformancePoints;
            }

            if (TotalScore < score.TotalScore)
            {
                TotalScore = score.TotalScore;
            }

            if (Combo < score.MaxCombo)
            {
                Combo = score.MaxCombo;
            }

            float accuracy = score.CalculateAccuracy();
            if (Accuracy < accuracy)
            {
                Accuracy = accuracy;
            }
        }

    }
}
