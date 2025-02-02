using OsuServer.Objects;

namespace OsuServer.State
{
    public class BanchoScores
    {
        private static int s_latestScoreId = 0;
        private Bancho _bancho;

        private Dictionary<int, SubmittedScore> IdToScore = new();
        private Dictionary<string, int> ChecksumToId = new();

        private Dictionary<int, string> IdToChecksum = new();

        public BanchoScores(Bancho bancho)
        {
            _bancho = bancho;
        }
        public async Task<SubmittedScore> Submit(Player player, Score score, string scoreChecksum)
        {
            if (IsSubmitted(scoreChecksum)) 
                return IdToScore[ChecksumToId[scoreChecksum]];

            int assignedScoreId = this.GetNextAvailableId();
            SubmittedScore submittedScore = new SubmittedScore(score, assignedScoreId, scoreChecksum);
            IdToScore.Add(assignedScoreId, submittedScore);

            IdToChecksum.Add(assignedScoreId, scoreChecksum);
            ChecksumToId.Add(scoreChecksum, assignedScoreId);

            // TODO: PP indexing for fast global top plays retrieval (when database is integrated)

            // Update the player's state based on this score
            await player.UpdateWithScore(submittedScore);

            return submittedScore;
        }

        public int GetId(string checksum)
        {
            return ChecksumToId[checksum];
        }

        public bool IsSubmitted(string checksum)
        {
            return ChecksumToId.ContainsKey(checksum);
        }

        public bool IsSubmitted(int id)
        {
            return IdToScore.ContainsKey(id);
        }

        public SubmittedScore? GetById(int id)
        {
            SubmittedScore? result = null;
            if (IdToScore.TryGetValue(id, out result)) 
                return result;
            return null;
        }

        public SubmittedScore? GetByChecksum(string scoreChecksum)
        {
            if (!IsSubmitted(scoreChecksum)) return null;
            return GetById(ChecksumToId[scoreChecksum]);
        }

        private int GetNextAvailableId()
        {
            // TODO: Temporary score ID assignment, implement this with database functionality
            return s_latestScoreId++;
        }
    }
}
