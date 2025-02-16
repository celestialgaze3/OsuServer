using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;
using Action = OsuServer.Objects.Action;

namespace OsuServer.API.Packets.Client
{
    public class UserUpdatePacketHandler : ClientPacketHandler
    {
        public UserUpdatePacketHandler(byte[] data) 
            : base((int) ClientPacketType.UserUpdate, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);
            if (player == null) return Task.CompletedTask;

            // Update this player with the new information
            player.Status.Action = (Action) reader.ReadByte();
            player.Status.InfoText = reader.ReadOsuString();
            player.Status.MapMD5 = reader.ReadOsuString();
            player.Status.Mods = new Mods(reader.ReadInt32());
            player.Status.GameMode = ((GameMode) reader.ReadByte()).WithMods(player.Status.Mods);
            player.Status.MapID = reader.ReadInt32();

            // Broadcast this update to all players
            bancho.BroadcastUserUpdate(player);
            return Task.CompletedTask;
        }
    }
}
