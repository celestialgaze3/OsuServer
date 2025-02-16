using OsuServer.State;
using System.Diagnostics.CodeAnalysis;

namespace OsuServer.Objects
{
    public class MatchSlot
    {
        public Match Match { get; }
        public OnlinePlayer? Player { get; set; }
        public SlotStatus Status { get; set; }
        public SlotTeam Team { get; set; }
        public Mods Mods { get; set; }
        public bool HasLoaded { get; set; }
        public bool HasSkipped { get; set; }

        public MatchSlot(Match match)
        {
            Match = match;
            Clear();
        }

        [MemberNotNull(nameof(Status), nameof(Team), nameof(Mods), nameof(HasLoaded), nameof(HasSkipped))]
        public void Clear()
        {
            if (Player != null)
            {
                Player.LeaveChannel(Match.Channel); // Kick player from match channel
                Match.SendUpdate(Player); // Make sure client knows they are kicked from the match
            }

            Player = null;
            Status = SlotStatus.Open;
            Team = SlotTeam.None;
            Mods = new Mods();
            HasLoaded = false;
            HasSkipped = false;
        }

        public void CopyFrom(MatchSlot other)
        {
            Player = other.Player;
            Status = other.Status;
            Team = other.Team;
            Mods = other.Mods;
            HasLoaded = other.HasLoaded;
            HasSkipped = other.HasSkipped;
        }

        public enum SlotTeam
        {
            None = 0,
            Blue = 1,
            Red = 2
        }

        public enum SlotStatus
        {
            Open = 1 << 0,
            Locked = 1 << 1,
            NotReady = 1 << 2,
            Ready = 1 << 3,
            NoMap = 1 << 4,
            Playing = 1 << 5,
            Complete = 1 << 6,
            Quit = 1 << 7
        }
    }
}
