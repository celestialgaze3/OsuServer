using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchCreatePacketHandler : ClientPacketHandler
    {
        public MatchCreatePacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchCreate, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            MatchData matchData = reader.ReadMatchData();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (player.IsInMatch) return; // Duplicate request

            // Create the match
            Match? match = bancho.Matches.Create(player, matchData);

            if (match == null)
            {
                player.Connection.AddPendingPacket(
                    new NotificationPacket(
                        $"The lobby could not be created as {bancho.Name} has reached the maximum number of matches."
                    )
                );
            }

            Console.WriteLine($"{player.Username} created match with ID {match.Id}");
        }
    }
}
