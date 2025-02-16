using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchChangeTeamPacketHandler : ClientPacketHandler
    {
        public MatchChangeTeamPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchChangeTeam, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;
            if (!player.IsInMatch) return Task.CompletedTask;
            Match match = player.Match;

            // Get the player's slot
            MatchSlot? slot = match.GetSlotWithPlayer(player.Id);
            if (slot == null) return Task.CompletedTask;

            match.ToggleTeam(slot);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} toggled their team to {slot.Team} in match ID {match.Id}");
            return Task.CompletedTask;
        }
    }
}