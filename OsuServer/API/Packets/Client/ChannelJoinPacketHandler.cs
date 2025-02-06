using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ChannelJoinPacketHandler : ClientPacketHandler
    {
        public ChannelJoinPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.ChannelJoin, data, osuToken, bancho) { }

        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            string channelName = reader.ReadOsuString().Substring(1); // Remove "#" at beginning
            Channel? channel = Bancho.GetChannel(channelName);
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;

            if (channel == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Unable to join channel #{channelName}; not found", Token, Bancho));
                Console.WriteLine($"{player.Username} tried to join non-existent channel #{channelName}");
                return Task.CompletedTask;
            }

            if (player.JoinChannel(channel))
            {
                player.Connection.AddPendingPacket(new ChannelJoinSuccessPacket(channel.Name, Token, Bancho));
            }
            else
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"You do not have permission to join #{channelName}", Token, Bancho));
            }

            return Task.CompletedTask;
        }
    }
}
