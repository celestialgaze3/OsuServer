using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.External.Database.Rows
{
    // TODO: some of the functions in this can be moved out/simplified
    public class DbScore : DbRow
    {
        public DbColumn<uint> Id { get; }
        public DbColumn<uint> AccountId { get; }
        public DbColumn<uint> BeatmapId { get; }
        public DbColumn<int> Perfects { get; }
        public DbColumn<int> Goods { get; }
        public DbColumn<int> Bads { get; }
        public DbColumn<int> Gekis { get; }
        public DbColumn<int> Katus { get; }
        public DbColumn<int> Misses { get; }
        public DbColumn<int> TotalScore { get; }
        public DbColumn<int> MaxCombo { get; }
        public DbColumn<bool> IsPerfectCombo { get; }
        public DbColumn<int> Mods { get; }
        public DbColumn<bool> IsPass { get; }
        public DbColumn<byte> GameMode { get; }
        public DbColumn<double> PP { get; }
        public DbColumn<bool> IsBestPP { get; }
        public DbColumn<bool> IsBestAccuracy { get; }
        public DbColumn<bool> IsBestCombo { get; }
        public DbColumn<bool> IsBestScore { get; }
        public DbColumn<bool> IsBestModdedScore { get; }
        public DbColumn<long> SubmittedTime { get; }
        public DbBlobColumn ReplayData { get; }

        public DbScore(uint id, uint accountId, uint beatmapId, int perfects, int goods, int bads, int gekis, int katus, int misses,
            int totalScore, int maxCombo, bool isPerfectCombo, int mods, bool isPass, byte gameMode, double pp, bool isBestPP,
            bool isBestAccuracy, bool isBestCombo, bool isBestScore, bool isBestModdedScore, long timestamp, 
            byte[]? replayData = null)
        {
            Id = new("id", id, false);
            AccountId = new("account_id", accountId);
            BeatmapId = new("beatmap_id", beatmapId);
            Perfects = new("perfects", perfects);
            Goods = new("goods", goods);
            Bads = new("bads", bads);
            Gekis = new("gekis", gekis);
            Katus = new("katus", katus);
            Misses = new("misses", misses);
            TotalScore = new("total_score", totalScore);
            MaxCombo = new("max_combo", maxCombo);
            IsPerfectCombo = new("is_perfect_combo", isPerfectCombo);
            Mods = new("mods", mods);
            IsPass = new("is_pass", isPass);
            GameMode = new("gamemode", gameMode);
            PP = new("pp", pp);
            IsBestPP = new("is_best_pp", isBestPP);
            IsBestAccuracy = new("is_best_accuracy", isBestAccuracy);
            IsBestCombo = new("is_best_combo", isBestCombo);
            IsBestScore = new("is_best_score", isBestScore);
            IsBestModdedScore = new("is_best_modded_score", isBestModdedScore);
            SubmittedTime = new("submitted_time", timestamp);
            ReplayData = new("replay_data", replayData);
        }

        protected DbScore(uint id, Score score, bool isBestPP, bool isBestAccuracy, bool isBestCombo, bool isBestScore,
            bool isBestModdedScore, byte[]? replayData = null)
        {
            Id = new("id", id, false);
            AccountId = new("account_id", (uint) score.Player.Id);
            BeatmapId = new("beatmap_id", (uint) score.Beatmap.Info.Id);
            Perfects = new("perfects", score.Perfects);
            Goods = new("goods", score.Goods);
            Bads = new("bads", score.Bads);
            Gekis = new("gekis", score.Gekis);
            Katus = new("katus", score.Katus);
            Misses = new("misses", score.Misses);
            TotalScore = new("total_score", score.TotalScore);
            MaxCombo = new("max_combo", score.MaxCombo);
            IsPerfectCombo = new("is_perfect_combo", score.PerfectCombo);
            Mods = new("mods", score.Mods.IntValue);
            IsPass = new("is_pass", score.Passed);
            GameMode = new("gamemode", (byte) score.GameMode);
            PP = new("pp", score.Beatmap.CalculatePerformancePoints(score));
            IsBestPP = new("is_best_pp", isBestPP);
            IsBestAccuracy = new("is_best_accuracy", isBestAccuracy);
            IsBestCombo = new("is_best_combo", isBestCombo);
            IsBestScore = new("is_best_score", isBestScore);
            IsBestModdedScore = new("is_best_modded_score", isBestModdedScore);
            SubmittedTime = new("submitted_time", score.Timestamp);
            ReplayData = new("replay_data", replayData);
        }

        public async Task<Score> GetScore(OsuServerDb database, Bancho bancho)
        {
            return new Score(Perfects.Value, Goods.Value, Bads.Value, Gekis.Value, Katus.Value,
            Misses.Value, TotalScore.Value, MaxCombo.Value, IsPerfectCombo.Value, new Mods(Mods.Value), IsPass.Value,
            (GameMode) GameMode.Value, new Player((int)AccountId.Value), 
            await bancho.GetBeatmap(database, null, (int)BeatmapId.Value), SubmittedTime.Value);
        }
        
        /// <summary>
        /// Prepares a score for insertion into the database
        /// </summary>
        /// <param name="database">The database instance</param>
        /// <param name="score">The Score to insert</param>
        /// <returns>A tuple of the DbScore to insert, with an array of all previous best scores in order of
        /// best pp, accuracy, combo, score, then modded score</returns>
        public static async Task<(DbScore, DbScore?[])> PrepareInsertion(OsuServerDb database, Score score, byte[]? replayBytes)
        {
            /* We want to find out if this play has bested previous plays in various stats. 
             * Let's make some queries to find out. */

            // Track this score's insertion data
            bool isBestPP = false;
            bool isBestAccuracy = false;
            bool isBestCombo = false;
            bool isBestScore = false;
            bool isBestModdedScore = false;

            // Find the best scores in each category
            DbScore? bestPP = 
                await GetTopScoreAsync(database, score.Beatmap.Info.Id, score.Player.Id, score.GameMode, "is_best_pp");
            DbScore? bestAccuracy = 
                await GetTopScoreAsync(database, score.Beatmap.Info.Id, score.Player.Id, score.GameMode, "is_best_accuracy");
            DbScore? bestCombo = 
                await GetTopScoreAsync(database, score.Beatmap.Info.Id, score.Player.Id, score.GameMode, "is_best_combo");
            DbScore? bestScore = 
                await GetTopScoreAsync(database, score.Beatmap.Info.Id, score.Player.Id, score.GameMode, "is_best_score");

            // For mod leaderboards
            DbScore? bestModdedScore =
                await GetTopModdedScoreAsync(database, score.Beatmap.Info.Id, score.Player.Id, score.Mods, score.GameMode);

            /* If this play is a fail, we don't want to overwrite existing scores, but we do still
             * want to save it. */
            if (!score.Passed)
            {
                // Ready to insert the new score!
                return (
                    new DbScore(0, score, false, false, false, false, false),
                    [bestPP, bestAccuracy, bestCombo, bestScore, bestModdedScore]
                );
            }

            // Overwrite best pp score if this score is better
            if (bestPP != null)
            {
                if (score.Beatmap.CalculatePerformancePoints(score) > bestPP.PP.Value)
                {
                    isBestPP = true;
                    bestPP.IsBestPP.Value = false;
                    await database.Score.UpdateOneAsync(bestPP);
                }
            } else isBestPP = true; // No previous score exists, or maybe a cheated score was deleted

            // Overwrite best accuracy score if this score is better
            if (bestAccuracy != null)
            {
                double scoreAccuracyValue = score.CalculateAccuracy();
                double bestAccuracyValue = Score.CalculateAccuracy(
                    bestAccuracy.Perfects.Value,
                    bestAccuracy.Goods.Value,
                    bestAccuracy.Bads.Value,
                    bestAccuracy.Misses.Value
                );
                if (scoreAccuracyValue > bestAccuracyValue)
                {
                    isBestAccuracy = true;
                    bestAccuracy.IsBestAccuracy.Value = false;
                    await database.Score.UpdateOneAsync(bestAccuracy);
                }
            } else isBestAccuracy = true;

            // Overwrite best combo score if this score is better
            if (bestCombo != null)
            {
                if (score.MaxCombo > bestCombo.MaxCombo.Value)
                {
                    isBestCombo = true;
                    bestCombo.IsBestCombo.Value = false;
                    await database.Score.UpdateOneAsync(bestCombo);
                }
            } else isBestCombo = true;

            // Overwrite best score score if this score's score is better (lol)
            if (bestScore != null)
            {
                if (score.TotalScore > bestScore.TotalScore.Value)
                {
                    isBestScore = true;
                    bestScore.IsBestScore.Value = false;
                    await database.Score.UpdateOneAsync(bestScore);
                }
            } else isBestScore = true;

            // Overwrite best modded score (with same mods) if this score's score is better
            if (bestModdedScore != null)
            {
                if (score.TotalScore > bestModdedScore.TotalScore.Value)
                {
                    isBestModdedScore = true;
                    bestModdedScore.IsBestModdedScore.Value = false;
                    await database.Score.UpdateOneAsync(bestModdedScore);
                }
            }
            else isBestModdedScore = true;

            // Ready to insert the new score!
            return (
                new DbScore(0, score, isBestPP, isBestAccuracy, isBestCombo, isBestScore, isBestModdedScore, replayBytes),
                [bestPP, bestAccuracy, bestCombo, bestScore, bestModdedScore]
            );
        }


        public static async Task<List<DbScore>> GetTopScoresAsync(OsuServerDb database, int beatmapId,
            GameMode gameMode)
        {
            return await database.Score.FetchManyAsync(
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND is_best_score = 1 AND gamemode = @gamemode",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = gameMode
                    }
                ),
                new DbClause(
                    "ORDER BY",
                    $"total_score DESC"
                ),
                new DbClause(
                    "LIMIT",
                    "50"
                )
            );
        }

        public static async Task<List<DbScore>> GetFriendTopScoresAsync(OsuServerDb database, int selfId, int beatmapId,
            GameMode gameMode)
        {
            return await database.Score.FetchManyAsync(
                new DbClause(
                    "INNER JOIN Friend ON",
                    "Friend.id = @self_id",
                    new() { ["self_id"] = selfId }
                ),
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND Friend.friend_id = Score.account_id AND is_best_score = 1 " +
                    "AND gamemode = @gamemode",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = gameMode
                    }
                ),
                new DbClause(
                    "ORDER BY",
                    $"total_score DESC"
                ),
                new DbClause(
                    "LIMIT",
                    "50"
                )
            );
        }

        public static async Task<List<DbScore>> GetModdedTopScoresAsync(OsuServerDb database, int beatmapId, Mods mods,
            GameMode gameMode)
        {
            return await database.Score.FetchManyAsync(
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND gamemode = @gamemode AND is_best_modded_score = 1 AND mods = @mods",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = gameMode,
                        ["mods"] = mods.IntValue
                    }
                ),
                new DbClause(
                    "ORDER BY",
                    $"total_score DESC"
                ),
                new DbClause(
                    "LIMIT",
                    "50"
                )
            );
        }

        public static async Task<long> GetScoreCountAsync(OsuServerDb database, int beatmapId, GameMode gameMode)
        {
            return await database.Score.GetRowCountAsync(
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND is_best_score = 1 AND gamemode = @gamemode",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = (int)gameMode
                    }
                )
            );
        }

        public static async Task<long> GetFriendScoreCountAsync(OsuServerDb database, int selfId, int beatmapId, GameMode gameMode)
        {
            return await database.Score.GetRowCountAsync(
                 new DbClause(
                    "INNER JOIN Friend ON",
                    "Friend.id = @self_id",
                    new() { ["self_id"] = selfId }
                ),
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND Friend.friend_id = Score.account_id AND is_best_score = 1 " +
                    "AND is_pass = 1 AND gamemode = @gamemode",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = gameMode
                    }
                )
            );
        }

        public static async Task<long> GetModdedScoreCountAsync(OsuServerDb database, Mods mods, int beatmapId, GameMode gameMode)
        {
            return await database.Score.GetRowCountAsync(
                new DbClause(
                    "WHERE",
                    "beatmap_id = @beatmap_id AND mods = @mods AND is_best_modded_score = 1 AND is_pass = 1 AND gamemode = @gamemode",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["gamemode"] = gameMode,
                        ["mods"] = mods.IntValue
                    }
                )
            );
        }

        public static async Task<DbScore?> GetTopScoreAsync(OsuServerDb database, int beatmapId, 
            int playerId, GameMode gameMode, string stat = "is_best_score")
        {
            return await database.Score.FetchOneAsync(
                new DbClause(
                    "WHERE",
                    $"beatmap_id = @beatmap_id AND account_id = @account_id " +
                    $"AND {stat} = 1 AND gamemode = @gamemode AND is_pass = 1",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["account_id"] = playerId,
                        ["gamemode"] = (int)gameMode
                    }
                )
            );
        }

        public static async Task<DbScore?> GetTopModdedScoreAsync(OsuServerDb database, int beatmapId,
            int playerId, Mods mods, GameMode gameMode)
        {
            return await database.Score.FetchOneAsync(
                new DbClause(
                    "WHERE",
                    $"beatmap_id = @beatmap_id AND account_id = @account_id AND mods = @mods " +
                    $"AND gamemode = @gamemode AND is_best_modded_score = 1",
                    new()
                    {
                        ["beatmap_id"] = beatmapId,
                        ["account_id"] = playerId,
                        ["mods"] = mods.IntValue,
                        ["gamemode"] = (int)gameMode
                    }
                )
            );
        }

        public static async Task<int> GetLeaderboardRank(OsuServerDb database, DbScore? score, int beatmapId, 
            GameMode gameMode)
        {
            int rank = 0;
            if (score != null)
            {
                rank = await database.Score.GetRankAsync(
                    score,
                    "total_score",
                    $"beatmap_id = {beatmapId} AND (is_best_score = 1 OR id={score.Id.Value}) " +
                    $"AND gamemode = {(int)gameMode}"
                );
            }

            return rank;
        }

        public static async Task<int> GetFriendLeaderboardRank(OsuServerDb database, int selfId, DbScore? score, int beatmapId,
            GameMode gameMode)
        {
            int rank = 0;
            if (score != null)
            {
                rank = await database.Score.GetRankAsync(
                    score,
                    "total_score",
                    $"beatmap_id = {beatmapId} AND ((Friend.friend_id = Score.account_id AND is_best_score = 1) OR Score.id={score.Id.Value}) " +
                    $"AND gamemode = {(int)gameMode}",
                    $"INNER JOIN Friend ON Friend.id = {selfId}"
                );
            }

            return rank;
        }

        public static async Task<int> GetModdedLeaderboardRank(OsuServerDb database, Mods mods, DbScore? score, int beatmapId,
            GameMode gameMode)
        {
            int rank = 0;
            if (score != null)
            {
                rank = await database.Score.GetRankAsync(
                    score,
                    "total_score",
                    $"beatmap_id = {beatmapId} AND mods = {mods.IntValue} AND (is_best_modded_score = 1 OR Score.id={score.Id.Value}) " +
                    $"AND gamemode = {(int)gameMode}"
                );
            }

            return rank;
        }

        public static async Task<int> GetBestRank(OsuServerDb database, int beatmapId, int playerId, 
            GameMode gameMode)
        {
            DbScore? playerTopScore = await GetTopScoreAsync(database, beatmapId, playerId, gameMode);
            return await GetLeaderboardRank(database, playerTopScore, beatmapId, gameMode);
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, AccountId, BeatmapId, Perfects, Goods, Bads, Gekis, Katus, Misses, 
                TotalScore, MaxCombo, IsPerfectCombo, Mods, IsPass, GameMode, PP, IsBestPP,
                IsBestAccuracy, IsBestCombo, IsBestScore, IsBestModdedScore, SubmittedTime,
                ReplayData];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
