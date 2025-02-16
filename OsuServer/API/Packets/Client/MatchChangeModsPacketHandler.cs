using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchChangeModsPacketHandler : ClientPacketHandler
    {
        public MatchChangeModsPacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchChangeMods, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            Mods mods = new(reader.ReadInt32());

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;
            Match match = player.Match;

            // Handle this mod change
            match.PlayerChangeMods(player, mods);
            match.BroadcastUpdate();

            Console.WriteLine($"{player.Username} changed their mods for match ID {match.Id}");
        }
    }
}
