using OsuServer.External.Database;
using OsuServer.External.Database.Rows;
using OsuServer.External.Database.Tables;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class PlayerStats
    {

        private OnlinePlayer _player;
        private ProfileStats _profileStats;

        public ProfileStats Values
        {
            get
            {
                return _profileStats.Clone();
            }
        }

        public PlayerStats(OnlinePlayer player) {
            _player = player;
            _profileStats = new ProfileStats();
        }

        public async Task UpdateWith(OsuServerDb database, SubmittedScore score)
        {
            // These stats are updated regardless of ranked status.
            _profileStats.Playcount += 1;
            _profileStats.TotalScore += score.TotalScore;

            // These stats should only be incremented if the score is a pass (TODO: on a ranked map)
            if (score.Passed)
            {
                _profileStats.RankedScore += score.TotalScore;

                // Update player's maximum combo if the score's combo exceeds their previous
                if (score.MaxCombo > _profileStats.MaxCombo)
                {
                    _profileStats.MaxCombo = score.MaxCombo;
                }
            }

            // Calculate and store the player's new total pp and accuracy
            _profileStats.PP = _player.Scores.CalculatePerformancePoints();
            _profileStats.Accuracy = _player.Scores.CalculateAccuracy();

            await SaveToDb(database);
        }

        public async Task<DbProfileStats?> GetDbRow(OsuServerDb database)
        {
            DbProfileStatsTable profileStats = database.ProfileStats;
            return await profileStats.FetchOneAsync(
                new DbClause(
                    "WHERE", 
                    "account_id = @account_id", 
                    new() { ["account_id"] = _player.Id }
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
            int rank = await database.ProfileStats.GetRankAsync(row, "pp");
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
