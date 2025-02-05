using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Org.BouncyCastle.Crypto;
using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OsuServer.State
{
    public class PlayerScores
    {
        private OnlinePlayer _player;
        private Bancho _bancho;
        private Dictionary<int, int> _scoreIds;
        private List<KeyValuePair<int, int>>? _sortedTopPlays;
        public GameMode GameMode { get; set; }
        public PlayerScores(OnlinePlayer player, Bancho bancho, GameMode gameMode)
        {
            _player = player;
            _bancho = bancho;
            GameMode = gameMode;
            _scoreIds = [];
        }

        public async Task<int> CalculatePlaycount(OsuServerDb database)
        {
            int playcount = (int)await database.Score.GetRowCountAsync(
                new DbClause(
                    "WHERE",
                    "gamemode = @gamemode AND account_id = @account_id",
                    new() { ["gamemode"] = (int)GameMode, ["account_id"] = _player.Id }
                )
            );
            Console.WriteLine($"Calculated {_player.Username}'s playcount to be {playcount}");
            return playcount;
        }

        public async Task<int> CalculateMaxCombo(OsuServerDb database)
        {
            int maxCombo = (int)await database.Score.MaxColumn<uint>("max_combo",
                new DbClause(
                    "WHERE",
                    "gamemode = @gamemode AND account_id = @account_id",
                    new() { ["gamemode"] = (int)GameMode, ["account_id"] = _player.Id }
                )
            );
            Console.WriteLine($"Calculated {_player.Username}'s max combo to be {maxCombo}");
            return maxCombo;
        }

        public async Task<long> CalculateTotalScore(OsuServerDb database)
        {
            long totalScore = (long)await database.Score.SumColumn<decimal>("total_score",
                new DbClause(
                    "WHERE",
                    "gamemode = @gamemode AND account_id = @account_id",
                    new() { ["gamemode"] = (int)GameMode, ["account_id"] = _player.Id }
                )
            );
            Console.WriteLine($"Calculated {_player.Username}'s total score to be {totalScore}");
            return totalScore;
        }

        public async Task<long> CalculateRankedScore(OsuServerDb database)
        {
            long rankedScore = (long)await database.Score.SumColumn<decimal>("total_score",
                new DbClause(
                    "WHERE",
                    "is_best_score = 1 AND gamemode = @gamemode AND account_id = @account_id",
                    new() { ["gamemode"] = (int)GameMode, ["account_id"] = _player.Id }
                )
            );
            Console.WriteLine($"Calculated {_player.Username}'s ranked score to be {rankedScore}");
            return rankedScore;
        }

        public double CalculatePerformancePoints()
        {
            int totalPasses = 0;
            double totalPP = 0.0f;
            if (_sortedTopPlays == null)
                return 0;

            foreach (var entry in _sortedTopPlays) 
            {
                int id = entry.Value;
                SubmittedScore? score = _bancho.Scores.GetById(id);

                if (score == null)
                {
                    Console.WriteLine($"Player contains null score with ID {id} ?");
                    continue;
                }

                if (!score.Passed || !score.Beatmap.ShouldAwardStatIncrease()) continue;

                double scorePP = score.Beatmap.CalculatePerformancePoints(score);
                totalPP += scorePP * Math.Pow(0.95f, totalPasses);
                totalPasses++;
            }

            return totalPP;
        }

        public double CalculateAccuracy()
        {
            int totalPasses = 0;
            double totalAccuracyWeighted = 0.0f;
            if (_sortedTopPlays == null)
                return 0;

            foreach (var entry in _sortedTopPlays)
            {
                int id = entry.Value;
                Score? score = _bancho.Scores.GetById(id);
                if (score == null)
                {
                    Console.WriteLine($"Player contains null score with ID {id} ?");
                    continue;
                }

                if (!score.Passed || !score.Beatmap.ShouldAwardStatIncrease()) continue;

                double scoreAccuracy = score.CalculateAccuracy();
                totalAccuracyWeighted += scoreAccuracy * Math.Pow(0.95d, totalPasses);
                totalPasses++;
            }

            return (double)(totalAccuracyWeighted * (1.0d / (20d * (1d - Math.Pow(0.95d, totalPasses)))));
        }

        /// <summary>
        /// Adds this to the list of best scores tracked by the player
        /// </summary>
        /// <param name="score">The score to add</param>
        public void Add(SubmittedScore score, bool sort = false)
        {
            int bestScoreId;
            SubmittedScore currentBestPP;

            // If the play is a fail, it's not relevant to any stat calculations and can be discarded.
            if (!score.Passed)
                return;

            // We only want one best pp score per beatmap
            // If no current best pp score exists, add the new score
            if (!_scoreIds.TryGetValue(score.Beatmap.Info.Id, out bestScoreId) || 
                !_bancho.Scores.TryGetById(bestScoreId, out currentBestPP))
            {
                _scoreIds[score.Beatmap.Info.Id] = score.Id;
                if (sort) SortTopPlays();
                return;
            }

            // If a current best pp score exists, add the new score only if it is better
            double currentBestPPValue = score.Beatmap.CalculatePerformancePoints(currentBestPP);
            double newPPValue = score.Beatmap.CalculatePerformancePoints(score);
            if (newPPValue >= currentBestPPValue)
            {
                _scoreIds[score.Beatmap.Info.Id] = score.Id;
            } else return;

            // Sort top plays for pp calculation
            if (sort)
                SortTopPlays();
        }

        public void SortTopPlays()
        {
            _sortedTopPlays = _scoreIds.ToList();
            _sortedTopPlays.Sort(Comparer<KeyValuePair<int, int>>.Create((first, second) =>
            {
                SubmittedScore? firstScore = _bancho.Scores.GetById(first.Value);
                SubmittedScore? secondScore = _bancho.Scores.GetById(second.Value);
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
