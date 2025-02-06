using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ChannelLeavePacketHandler : ClientPacketHandler
    {
        public ChannelLeavePacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.ChannelLeave, data, osuToken, bancho) { }

        protected override Task Handle(OsuServerDb database, BinaryReader reader)
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

            Channel? channel = Bancho.GetChannel(channelName);
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;

            if (channel == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Unable to leave channel #{channelName}; not found", Token, Bancho));
                Console.WriteLine($"{player.Username} tried to leave non-existent channel #{channelName}");
                return Task.CompletedTask;
            }

            player.LeaveChannel(channel);

            return Task.CompletedTask;
        }
    }
}
