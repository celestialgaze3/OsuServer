using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
namespace OsuServer.API.Packets.Client
{
    public class MatchChangeSlotStatusPacketHandler : ClientPacketHandler
    {
        private MatchSlot.SlotStatus _status;
        public MatchChangeSlotStatusPacketHandler(MatchSlot.SlotStatus status, int id, byte[] data) 
            : base(id, data) 
        {
            _status = status;
        }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Change the player's status
            match.ChangePlayerSlotStatus(player, _status);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} changed their status to {_status} in match ID {match.Id}");
        }
    }
}
