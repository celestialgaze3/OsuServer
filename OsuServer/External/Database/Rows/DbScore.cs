using Microsoft.AspNetCore.Mvc.Diagnostics;
using MySqlConnector;
using OsuServer.Objects;
using OsuServer.State;
using System.Xml.Linq;

namespace OsuServer.External.Database.Rows
{
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

        public DbScore(uint id, uint accountId, uint beatmapId, int perfects, int goods, int bads, int gekis, int katus, int misses,
            int totalScore, int maxCombo, bool isPerfectCombo, int mods, bool isPass, byte gameMode, double pp, bool isBestPP,
            bool isBestAccuracy, bool isBestCombo, bool isBestScore)
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
        }

        protected DbScore(uint id, Score score, bool isBestPP, bool isBestAccuracy, bool isBestCombo, bool isBestScore)
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
        }
        
        public static async Task<DbScore> PrepareInsertion(OsuServerDb database, Score score)
        {
            /* We want to find out if this play has bested previous plays in various stats. 
             * Let's make some queries to find out. */

            // Track this score's insertion data
            bool isBestPP = false;
            bool isBestAccuracy = false;
            bool isBestCombo = false;
            bool isBestScore = false;

            // Find the best scores in each category
            DbScore? bestPP = await GetBestScore(database, score.Player, score.Beatmap, "is_best_pp");
            DbScore? bestAccuracy = await GetBestScore(database, score.Player, score.Beatmap, "is_best_accuracy");
            DbScore? bestCombo = await GetBestScore(database, score.Player, score.Beatmap, "is_best_combo");
            DbScore? bestScore = await GetBestScore(database, score.Player, score.Beatmap, "is_best_score");

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

            // Ready to insert the new score!
            return new DbScore(0, score, isBestPP, isBestAccuracy, isBestCombo, isBestScore);
        }

        private static async Task<DbScore?> GetBestScore(OsuServerDb database, Player player, BanchoBeatmap beatmap, string category)
        {
            return await database.Score.FetchOneAsync(
                new DbClause(
                    "WHERE",
                    $"account_id = @account_id AND beatmap_id = @beatmap_id AND {category} = 1",
                    new()
                    {
                        ["account_id"] = player.Id,
                        ["beatmap_id"] = beatmap.Info.Id
                    }
                )
            );
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, AccountId, BeatmapId, Perfects, Goods, Bads, Gekis, Katus, Misses, 
                TotalScore, MaxCombo, IsPerfectCombo, Mods, IsPass, GameMode, PP, IsBestPP,
                IsBestAccuracy, IsBestCombo, IsBestScore];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
