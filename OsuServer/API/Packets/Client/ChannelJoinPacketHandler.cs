using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ChannelJoinPacketHandler : ClientPacketHandler
    {
        public ChannelJoinPacketHandler(byte[] data) 
            : base((int) ClientPacketType.ChannelJoin, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            string channelName = reader.ReadOsuString().Substring(1); // Remove "#" at beginning
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;
            Channel? channel = bancho.GetChannel(player, channelName);

            if (channel == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Unable to join channel #{channelName}; not found"));
                Console.WriteLine($"{player.Username} tried to join non-existent channel #{channelName}");
                return Task.CompletedTask;
            }

            if (player.JoinChannel(channel))
            {
                player.Connection.AddPendingPacket(new ChannelJoinSuccessPacket(channel.Name));
            }
            else
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"You do not have permission to join #{channelName}"));
            }

            return Task.CompletedTask;
        }
    }
}
