﻿using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.Database.Tables;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class PlayerStats
    {

        private OnlinePlayer _player;
        private ProfileStats _profileStats;

        public GameMode GameMode { get; set; }

        public ProfileStats Values
        {
            get
            {
                return _profileStats.Clone();
            }
        }

        public PlayerStats(OnlinePlayer player, GameMode mode) {
            _player = player;
            GameMode = mode;
            _profileStats = new ProfileStats();
        }

        public async Task UpdateWith(OsuServerDb database, SubmittedScore score)
        {
            // These stats are updated regardless of ranked status.
            _profileStats.Playcount += 1;
            _profileStats.TotalScore += score.TotalScore;

            // These stats should only be incremented if the score should award pp
            if (score.Passed && score.Beatmap.ShouldAwardStatIncrease())
            {
                _profileStats.RankedScore += score.TotalScore;

                // Update player's maximum combo if the score's combo exceeds their previous
                if (score.MaxCombo > _profileStats.MaxCombo)
                {
                    _profileStats.MaxCombo = score.MaxCombo;
                }
            }

            // Calculate and store the player's new total pp and accuracy
            RecalculateStats();

            await SaveToDb(database);
        }

        /// <summary>
        /// Recalculates all of this player's stats
        /// </summary>
        public void RecalculateStats()
        {
            _profileStats.PP = _player.Scores[GameMode].CalculatePerformancePoints();
            _profileStats.Accuracy = _player.Scores[GameMode].CalculateAccuracy();
            // TODO: ranked and maybe total score if we decide to store fails permanently
        }

        public async Task<DbProfileStats?> GetDbRow(OsuServerDb database)
        {
            DbProfileStatsTable profileStats = database.ProfileStats;
            return await profileStats.FetchOneAsync(
                new DbClause(
                    "WHERE", 
                    "account_id = @account_id AND gamemode = @gamemode", 
                    new() { ["account_id"] = _player.Id, ["gamemode"] = (int)GameMode }
                )
            );
        }

        public async Task UpdateFromDb(OsuServerDb database)
        {
            DbProfileStats? row = await GetDbRow(database);
            if (row == null) return;

            _profileStats = new ProfileStats(
                row.TotalScore.Value, 
                row.RankedScore.Value, 
                row.Accuracy.Value, 
                row.Playcount.Value, 
                row.Rank.Value, 
                row.PP.Value, 
                row.MaxCombo.Value
            );

            await UpdateRank(database, row);
        }

        public async Task UpdateRank(OsuServerDb database, DbProfileStats row)
        {
            int rank = await database.ProfileStats.GetRankAsync(row, "pp", $"gamemode = {(int)GameMode}");
            _profileStats.Rank = rank;
        }

        public async Task SaveToDb(OsuServerDb database)
        {
            DbProfileStatsTable table = database.ProfileStats;
            DbProfileStats? row = await GetDbRow(database);

            if (row == null)
            {
                row = new(
                    _player.Id,
                    GameMode,
                    _profileStats.TotalScore,
                    _profileStats.RankedScore,
                    _profileStats.Accuracy,
                    _profileStats.Playcount,
                    _profileStats.Rank,
                    _profileStats.PP,
                    _profileStats.MaxCombo
                );

                await table.InsertAsync(row);
            } 
            else
            {
                row.TotalScore.Value = _profileStats.TotalScore;
                row.RankedScore.Value = _profileStats.RankedScore;
                row.Accuracy.Value = _profileStats.Accuracy;
                row.Playcount.Value = _profileStats.Playcount;
                row.Rank.Value = _profileStats.Rank;
                row.PP.Value = _profileStats.PP;
                row.MaxCombo.Value = _profileStats.MaxCombo;

                await table.UpdateOneAsync(row);
            }

            await UpdateRank(database, row);
        }

    }
}
