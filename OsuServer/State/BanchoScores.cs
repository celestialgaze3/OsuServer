using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class BanchoScores
    {
        private Bancho _bancho;

        private Dictionary<int, SubmittedScore> _idToScore = new();
        private Dictionary<string, int> _checksumToId = new();
        private Dictionary<int, string> _idToChecksum = new();

        public BanchoScores(Bancho bancho)
        {
            _bancho = bancho;
        }
        public async Task<SubmittedScore> Submit(Player player, Score score, string scoreChecksum)
        {
            if (IsSubmitted(scoreChecksum)) 
                return _idToScore[_checksumToId[scoreChecksum]];

            DbScore dbScore = new(0, score);

            int assignedScoreId = await _bancho.Database.Score.InsertAsync(dbScore);
            SubmittedScore submittedScore = new(score, assignedScoreId);
            _idToScore.Add(assignedScoreId, submittedScore);

            _idToChecksum.Add(assignedScoreId, scoreChecksum);
            _checksumToId.Add(scoreChecksum, assignedScoreId);

            // TODO: PP indexing for fast global top plays retrieval (when database is integrated)

            // Update the player's state based on this score
            await player.UpdateWithScore(submittedScore);

            return submittedScore;
        }

        /// <summary>
        /// Loads a player's top 500 plays from the database (to calculate pp in real time)
        /// </summary>
        /// <param name="player">The player to get the scores from</param>
        /// <returns></returns>
        public async Task UpdateFromDb(Player player)
        {
            List<DbScore> scores = await _bancho.Database.Score.FetchManyAsync(
                new DbClause("WHERE", "account_id = @account_id", new() { ["account_id"] = player.Id }),
                new DbClause("ORDER BY", "pp"),
                new DbClause("LIMIT", "500")
            );

            foreach (DbScore dbScore in scores)
            {
                Score score = await Score.Get(_bancho, dbScore);
                int id = (int) dbScore.Id.Value;
                SubmittedScore submittedScore = new SubmittedScore(score, id);

                _idToScore.Add(id, submittedScore);
                player.Scores.Add(submittedScore);
            }

            Console.WriteLine($"Loaded {scores.Count} top plays");
        }

        public int GetId(string checksum)
        {
            return _checksumToId[checksum];
        }

        public bool IsSubmitted(string checksum)
        {
            return _checksumToId.ContainsKey(checksum);
        }

        public bool IsSubmitted(int id)
        {
            return _idToScore.ContainsKey(id);
        }

        public SubmittedScore? GetById(int id)
        {
            SubmittedScore? result = null;
            if (_idToScore.TryGetValue(id, out result)) 
                return result;
            return null;
        }

        public SubmittedScore? GetByChecksum(string scoreChecksum)
        {
            if (!IsSubmitted(scoreChecksum)) return null;
            return GetById(_checksumToId[scoreChecksum]);
        }
    }
}
