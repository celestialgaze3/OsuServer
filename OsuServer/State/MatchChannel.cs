namespace OsuServer.State
{
    public class MatchChannel : Channel
    {
        public const string VisibleName = "multiplayer";
        public MatchChannel(Match match, Bancho bancho)
            : base(VisibleName, $"For multiplayer lobby with ID {match.Id}", bancho) { }
    }
}
