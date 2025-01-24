using OsuServer.Objects;

namespace OsuServer.State
{
    public class PlayerStats
    {

        private Player _player;
        private ProfileStats _profileStats;

        public ProfileStats Values
        {
            get
            {
                return _profileStats.Clone();
            }
        }

        public PlayerStats(Player player) {
            _player = player;
            _profileStats = new ProfileStats();
        }

        public void UpdateWith(SubmittedScore score)
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
        }

    }
}
