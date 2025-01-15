﻿using OsuServer.API.Packets.Server;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class ToggleBlockMessagesPacketHandler : ClientPacketHandler
    {
        public ToggleBlockMessagesPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.ToggleBlockMessages, data, osuToken, bancho) { }

        /// <summary>
        /// Client has toggled the in-game setting to block messages from non-friends
        /// </summary>
        protected override void Handle(ref BinaryReader reader)
        {
            bool blocked = reader.ReadInt32() != 0;
            Player? player = Bancho.GetPlayer(Token);

            if (player == null) return;

            player.BlockingStrangerMessages = blocked;

            string blockedString = player.Username + " " + (blocked ? "is now " : "is no longer") + " blocking messages from non-friends.";
            Console.WriteLine(blockedString);
        }
    }
}
