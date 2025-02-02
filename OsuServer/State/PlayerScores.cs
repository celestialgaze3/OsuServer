using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class PlayerScores
    {
        private Player _player;
        private Bancho _bancho;
        private List<int> _scoreIds;
        public PlayerScores(Player player, Bancho bancho)
        {
            _player = player;
            _bancho = bancho;

            _scoreIds = [];
        }

        public double CalculatePerformancePoints()
        {
            int totalPasses = 0;
            double totalPP = 0.0f;
            for (int i = 0; i < _scoreIds.Count; i++) 
            {
                int id = _scoreIds[i];
                SubmittedScore? score = _bancho.Scores.GetById(id);

                if (score == null)
                {
                    Console.WriteLine($"Player contains null score with ID {id} ?");
                    continue;
                }

                if (!score.Passed) continue;

                totalPP += (double)(score.Beatmap.CalculatePerformancePoints(score) * Math.Pow(0.95f, totalPasses));
                totalPasses++;
            }

            return totalPP;
        }

        public double CalculateAccuracy()
        {
            int totalPasses = 0;
            double totalAccuracyWeighted = 0.0f;

            for (int i = 0; i < _scoreIds.Count; i++)
            {
                int id = _scoreIds[i];
                Score? score = _bancho.Scores.GetById(id);
                if (score == null)
                {
                    Console.WriteLine($"Player contains null score with ID {id} ?");
                    continue;
                }

                if (!score.Passed) continue;

                totalAccuracyWeighted += (double)(score.CalculateAccuracy() * Math.Pow(0.95d, totalPasses));
                totalPasses++;
            }

            return (double)(totalAccuracyWeighted * (1.0d / (20d * (1d - Math.Pow(0.95d, totalPasses)))));
        }

        /// <summary>
        /// Updates a player's stats based on a submitted score
        /// </summary>
        /// <param name="score">The score this player set</param>
        public void Add(SubmittedScore score)
        {
            _scoreIds.Add(score.Id);

            _scoreIds.Sort(Comparer<int>.Create((first, second) =>
            {
                SubmittedScore? firstScore = _bancho.Scores.GetById(first);
                SubmittedScore? secondScore = _bancho.Scores.GetById(second);
                if (firstScore == null)
                    return 1;
                if (secondScore == null)
                    return -1;

                return secondScore.Beatmap.CalculatePerformancePoints(secondScore)
                    .CompareTo(firstScore.Beatmap.CalculatePerformancePoints(firstScore));
            }));
        }

        public void GetScoreRanking(int scoreId)
        {
            //TopPlays.
        }
    }
}
