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

            _scoreIds = new List<int>();
            /*
            ScoreIds = new List<int>();

            // Sort top plays in reverse order (biggest values first)
            TopPlays = new SortedSet<int>(
                
            );*/
        }

        public float CalculatePerformancePoints()
        {

            float totalPP = 0.0f;
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

                totalPP += (float)(score.PerformancePoints * Math.Pow(0.95f, i));
            }

            return totalPP;
        }

        public float CalculateAccuracy()
        {
            int totalPasses = 0;
            float totalAccuracyWeighted = 0.0f;

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

                totalAccuracyWeighted += (float)(score.CalculateAccuracy() * Math.Pow(0.95f, totalPasses));
                totalPasses++;
            }

            return (float)(totalAccuracyWeighted * (1.0f / (20f * (1f - Math.Pow(0.95f, totalPasses)))));
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

                return secondScore.PerformancePoints.CompareTo(firstScore.PerformancePoints);
            }));
        }

        public void GetScoreRanking(int scoreId)
        {
            //TopPlays.
        }
    }
}
