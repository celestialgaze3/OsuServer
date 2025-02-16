using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MessageUserPacketHandler : ClientPacketHandler
    {
        public MessageUserPacketHandler(byte[] data) : 
            base((int) ClientPacketType.MessageUser, data) { }

        protected override async Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return;

            OsuMessage message = reader.ReadOsuMessage();

            // Fill message with sender info
            message.Sender = player.Username;
            message.SenderId = player.Id;

            string content = message.Text.Trim();

            OnlinePlayer? recipient = bancho.GetPlayerByUsername(message.Recipient);

            if (recipient == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Your message could not sent to {message.Recipient} " +
                    $"as the player could not be found."));
                return;
            }
            
            if (content.Length == 0) return;

            const int MaxMessageLength = 2000;
            if (content.Length > MaxMessageLength)
            {
                content = content.Substring(0, MaxMessageLength);
                player.Connection.AddPendingPacket(new NotificationPacket($"Your message was truncated as it exceeded " +
                    $"{MaxMessageLength} characters."));
            }

            // Respect "Block private messages from non-friends" setting
            if (recipient.BlockingStrangerMessages && !await recipient.HasFriended(database, player))
            {
                Console.WriteLine($"{message.Sender} tried to message {recipient.Username}, but DMs were blocked from non-friends.");
                player.Connection.AddPendingPacket(new NotificationPacket($"You cannot message {recipient.Username} because " +
                    "they are blocking messages from non-friends."));
                return;
            }
            
            if (player.BlockingStrangerMessages && !await player.HasFriended(database, recipient))
            {
                Console.WriteLine($"{message.Sender} tried to message {recipient.Username}, but their own DMs were " +
                    $"blocked from non-friends and they don't have {recipient.Username} added..");
                player.Connection.AddPendingPacket(new NotificationPacket($"You cannot message {recipient.Username} because " +
                    "you are blocking messages from non-friends and they will not be able to reply to you."));
                return;
            }

            // Forward message to channel
            recipient.SendMessage(message);

            Console.WriteLine($"[{message.Sender} -> {recipient.Username}]: {content}");
        }
    }
}
