using OsuServer.Objects;

namespace OsuServer.State
{
    public class SubmittedScore : Score
    {
        public int Id { get; set; }
        public SubmittedScore(Score score, int id) : 
            base(score.Perfects, score.Goods, score.Bads, score.Gekis, score.Katus, 
                score.Misses, score.TotalScore, score.MaxCombo, score.PerfectCombo,
                score.Mods, score.Passed, score.GameMode, score.Player, score.Beatmap, 
                score.Timestamp)
        {
            Id = id;
        }
    }
}
