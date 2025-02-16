using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MatchInvitePacketHandler : ClientPacketHandler
    {
        public MatchInvitePacketHandler(byte[] data) 
            : base((int) ClientPacketType.MatchInvite, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            int id = reader.ReadInt32();

            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return;
            if (!player.IsInMatch) return;

            OnlinePlayer? invited = bancho.GetPlayer(id);
            if (invited == null) return;

            invited.Connection.AddPendingPacket(
                new MatchInvitePacket(
                    new OsuMessage(
                        player.Username,
                        $"Come join my match: [osump://{player.Match.Id}/{player.Match.Password} {player.Match.Name}]",
                        invited.Username,
                        player.Id
                    )
                )
            );
        }
    }
}
