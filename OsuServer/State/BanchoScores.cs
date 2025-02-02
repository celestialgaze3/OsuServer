using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;
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
        public async Task<SubmittedScore> Submit(OsuServerDb database, Player player, Score score, string scoreChecksum)
        {
            if (IsSubmitted(scoreChecksum)) 
                return _idToScore[_checksumToId[scoreChecksum]];

            await database.StartTransaction();
            DbScore dbScore = await DbScore.PrepareInsertion(database, score);

            int assignedScoreId = await database.Score.InsertAsync(dbScore);
            await database.CommitTransaction();

            SubmittedScore submittedScore = new(score, assignedScoreId);
            _idToScore.Add(assignedScoreId, submittedScore);
            _idToChecksum.Add(assignedScoreId, scoreChecksum);
            _checksumToId.Add(scoreChecksum, assignedScoreId);

            // Update the player's state based on this score
            await player.UpdateWithScore(database, submittedScore);

            return submittedScore;
        }

        /// <summary>
        /// Loads a player's top 500 plays from the database (to calculate pp in real time)
        /// </summary>
        /// <param name="player">The player to get the scores from</param>
        /// <returns></returns>
        public async Task UpdateFromDb(OsuServerDb database, Player player)
        {
            List<DbScore> scores = await database.Score.FetchManyAsync(
                new DbClause(
                    "WHERE", 
                    "account_id = @account_id AND is_best_pp = 1", 
                    new() { ["account_id"] = player.Id }
                ),
                new DbClause("ORDER BY", "pp"),
                new DbClause("LIMIT", "500")
            );

            foreach (DbScore dbScore in scores)
            {
                Score score = await Score.Get(_bancho, dbScore);
                int id = (int) dbScore.Id.Value;
                SubmittedScore submittedScore = new SubmittedScore(score, id);

                _idToScore[id] = submittedScore;
                score.Beatmap.AddScore(player, submittedScore);
                player.Scores.Add(submittedScore);
            }

            player.Scores.SortTopPlays();
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

        public bool TryGetById(int id, out SubmittedScore result)
        {
            return _idToScore.TryGetValue(id, out result);
        }

        public SubmittedScore? GetByChecksum(string scoreChecksum)
        {
            if (!IsSubmitted(scoreChecksum)) return null;
            return GetById(_checksumToId[scoreChecksum]);
        }
    }
}
