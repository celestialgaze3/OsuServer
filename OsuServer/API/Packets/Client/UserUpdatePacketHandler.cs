using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
using Action = OsuServer.Objects.Action;

namespace OsuServer.API.Packets.Client
{
    public class UserUpdatePacketHandler : ClientPacketHandler
    {
        public UserUpdatePacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.UserUpdate, data, osuToken, bancho) { }

        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            OnlinePlayer? player = Bancho.GetPlayer(Token);
            if (player == null) return Task.CompletedTask;

            // Update this player with the new information
            player.Status.Action = (Action) reader.ReadByte();
            player.Status.InfoText = reader.ReadOsuString();
            player.Status.MapMD5 = reader.ReadOsuString();
            player.Status.Mods = new Mods(reader.ReadInt32());
            player.Status.GameMode = (GameMode) reader.ReadByte();
            player.Status.MapID = reader.ReadInt32();

            // Broadcast this update to all players
            Bancho.BroadcastUserUpdate(player);
            return Task.CompletedTask;
        }
    }
}
