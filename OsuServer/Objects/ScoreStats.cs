using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.State;

namespace OsuServer.Objects
{
    public class ScoreStats
    {
        public double PerformancePoints { get; set; } = 0.0f;
        public int TotalScore { get; set; } = 0;
        public int Combo { get; set; } = 0;
        public double Accuracy { get; set; } = 0.0f;

        public ScoreStats(double performancePoints, int score, int combo, double accuracy)
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
            PerformancePoints = score.Beatmap.CalculatePerformancePoints(score);
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

        public static async Task<ScoreStats> FromDbScores(OsuServerDb database, Bancho bancho, params DbScore?[] scores)
        {
            SubmittedScore?[] submittedScores = new SubmittedScore[scores.Length];
            for (int i = 0; i < scores.Length; i++) { 
                var dbScore = scores[i];
                if (dbScore == null)
                {
                    submittedScores[i] = null;
                    continue;
                }

                submittedScores[i] = new SubmittedScore(await dbScore.GetScore(database, bancho), (int)dbScore.Id.Value);
            }

            return new ScoreStats(submittedScores);
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

        private void SetBestValues(Score? score)
        {
            if (score == null) return;

            if (PerformancePoints < score.Beatmap.CalculatePerformancePoints(score))
            {
                PerformancePoints = score.Beatmap.CalculatePerformancePoints(score);
            }

            if (TotalScore < score.TotalScore)
            {
                TotalScore = score.TotalScore;
            }

            if (Combo < score.MaxCombo)
            {
                Combo = score.MaxCombo;
            }

            double accuracy = score.CalculateAccuracy();
            if (Accuracy < accuracy)
            {
                Accuracy = accuracy;
            }
        }

    }
}
