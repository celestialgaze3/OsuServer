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

        /// <summary>
        /// Submits a score to this Bancho instance
        /// </summary>
        /// <param name="database">The database instance</param>
        /// <param name="player">The player who set the score</param>
        /// <param name="score">The score itself</param>
        /// <param name="scoreChecksum">The checksum submitted by the client</param>
        /// <returns>A tuple with three items: the first one is the SubmittedScore, the second is the DbScore,
        /// and the third is an array of the old best scores</returns>
        public async Task<(SubmittedScore, DbScore?, DbScore?[])> Submit(OsuServerDb database, OnlinePlayer player, Score score, string scoreChecksum)
        {
            if (IsSubmitted(scoreChecksum))
            {
                SubmittedScore foundScore = _idToScore[_checksumToId[scoreChecksum]];
                return (foundScore, null, []);
            }

            await database.StartTransaction();
            (DbScore, DbScore?[]) dbScore = await DbScore.PrepareInsertion(database, score);

            int assignedScoreId = await database.Score.InsertAsync(dbScore.Item1, false);
            await database.CommitTransaction();

            dbScore.Item1.Id.Value = (uint) assignedScoreId;

            SubmittedScore submittedScore = new(score, assignedScoreId);
            _idToScore.Add(assignedScoreId, submittedScore);
            _idToChecksum.Add(assignedScoreId, scoreChecksum);
            _checksumToId.Add(scoreChecksum, assignedScoreId);

            // Update the player's state based on this score
            await player.UpdateWithScore(database, submittedScore);

            return (submittedScore, dbScore.Item1, dbScore.Item2);
        }

        /// <summary>
        /// Loads a player's top 500 plays from the database (to calculate pp in real time)
        /// </summary>
        /// <param name="player">The player to get the scores from</param>
        /// <returns></returns>
        public async Task UpdateFromDb(OsuServerDb database, OnlinePlayer player, GameMode gameMode)
        {
            List<DbScore> scores = await database.Score.FetchManyAsync(
                new DbClause(
                    "WHERE", 
                    "account_id = @account_id AND is_best_pp = 1 AND gamemode = @gamemode", 
                    new() { ["account_id"] = player.Id, ["gamemode"] = (int)gameMode }
                ),
                new DbClause("ORDER BY", "pp"),
                new DbClause("LIMIT", "500")
            );

            foreach (DbScore dbScore in scores)
            {
                Score score = await Score.Get(database, _bancho, dbScore);
                int id = (int) dbScore.Id.Value;
                SubmittedScore submittedScore = new(score, id);

                _idToScore[id] = submittedScore;
                player.Scores[score.GameMode].Add(submittedScore);
            }

            player.Scores[gameMode].SortTopPlays();
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
