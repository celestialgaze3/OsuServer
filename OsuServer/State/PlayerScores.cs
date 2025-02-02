﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Org.BouncyCastle.Crypto;
using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using System.Collections.Generic;

namespace OsuServer.State
{
    public class PlayerScores
    {
        private Player _player;
        private Bancho _bancho;
        private Dictionary<int, int> _scoreIds;
        private List<KeyValuePair<int, int>>? _sortedTopPlays;
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
            foreach (var entry in _scoreIds) 
            {
                int id = entry.Value;
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

            foreach (var entry in _scoreIds)
            {
                int id = entry.Value;
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
        /// Adds this to the list of best scores tracked by the player
        /// </summary>
        /// <param name="score">The score to add</param>
        public void Add(SubmittedScore score, bool sort = false)
        {
            int bestScoreId;
            SubmittedScore currentBestPP;

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
