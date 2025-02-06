﻿using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class MessageChannelPacketHandler : ClientPacketHandler
    {
        public MessageChannelPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.MessageChannel, data, osuToken, bancho) { }

        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;

            OsuMessage message = reader.ReadOsuMessage();

            // Fill message with sender info
            message.Sender = player.Username;
            message.SenderId = player.Id;

            string content = message.Text.Trim();

            Channel? recipient = Bancho.GetChannel(message.Recipient.Substring(1)); // Remove leading "#"

            if (recipient == null)
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Your message could not sent to {message.Recipient} as the channel could not be found.", Token, Bancho));
                return Task.CompletedTask;
            }

            if (!recipient.HasMember(player)) 
            {
                player.Connection.AddPendingPacket(new NotificationPacket($"Your message could not sent to {message.Recipient} as you have not joined that channel.", Token, Bancho));
                return Task.CompletedTask;
            }

            if (content.Length == 0) return Task.CompletedTask;

            const int MaxMessageLength = 2000;
            if (content.Length > MaxMessageLength)
            {
                content = content.Substring(0, MaxMessageLength);
                player.Connection.AddPendingPacket(new NotificationPacket($"Your message was truncated as it exceeded {MaxMessageLength} characters.", Token, Bancho));
            }

            // Forward message to channel
            recipient.SendMessage(message);

            Console.WriteLine($"[#{recipient.Name}] {message.Sender}: {content}");
            return Task.CompletedTask;
        }
    }
}
