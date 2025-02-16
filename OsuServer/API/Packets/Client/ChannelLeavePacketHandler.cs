using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ChannelLeavePacketHandler : ClientPacketHandler
    {
        public ChannelLeavePacketHandler(byte[] data) 
            : base((int) ClientPacketType.ChannelLeave, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            string channelName = reader.ReadOsuString();

            if (channelName.StartsWith("#"))
            {
                channelName = channelName.Substring(1); // Remove #
            } else
            {
                // This is a user closing a DM with another user, where osu! still sends a "leave channel" packet, for some reason. Ignore.
                return Task.CompletedTask;
            }

            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;
            Channel? channel = bancho.GetChannel(player, channelName);

            if (channel == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Unable to leave channel #{channelName}; not found"));
                Console.WriteLine($"{player.Username} tried to leave non-existent channel #{channelName}");
                return Task.CompletedTask;
            }

            player.LeaveChannel(channel);

            return Task.CompletedTask;
        }
    }
}
