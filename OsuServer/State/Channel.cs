﻿using OsuServer.API.Packets.Server;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class Channel
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public HashSet<int> Members { get; private set; } = [];
        private Bancho _bancho;
        public bool ShouldAutoJoin { get; private set; } = false;

        public Channel(string name, string description, Bancho bancho)
        {
            Name = name;
            Description = description;
            _bancho = bancho;
        }

        /// <summary>
        /// Adds a player to this channel, if they are able to join
        /// </summary>
        /// <param name="player">The player to add</param>
        /// <returns>Whether or not the operation was successful</returns>
        public bool AddPlayer(OnlinePlayer player)
        {
            if (!CanJoin(player))
            {
                return false;
            }

            Members.Add(player.Id);
            OnJoin(player);
            return true;
        }

        // TODO: private channels
        public virtual bool CanJoin(OnlinePlayer player)
        {
            return true;
        }

        public Channel AutoJoin()
        {
            ShouldAutoJoin = true;
            return this;
        }

        private void OnJoin(OnlinePlayer player)
        {
            UpdateChannelInfoAll();
        }

        private void UpdateChannelInfoAll()
        {
            // Update the channel info (player count) for each member
            foreach (int memberId in Members)
            {
                UpdateChannelInfo(memberId);
            }
        }

        private void UpdateChannelInfo(int playerId)
        {
            OnlinePlayer? player = _bancho.GetPlayer(playerId);
            if (player == null) return;

            player.Connection.AddPendingPacket(new ChannelPacket(this));
        }

        public bool HasMember(OnlinePlayer player)
        {
            return Members.Contains(player.Id);
        }

        public void SendMessage(OsuMessage message)
        {
            foreach (int memberId in Members)
            {
                OnlinePlayer? member = _bancho.GetPlayer(memberId);
                if (member == null) continue;

                if (member.Id != message.SenderId)
                {
                    member.SendMessage(message);
                }
            }
        }

        public void RemovePlayer(OnlinePlayer player)
        {
            if (!Members.Contains(player.Id)) return;
            Members.Remove(player.Id);

            // Now that the player has been removed, we need to tell both the player that left and the players in the channel about the updated usercount
            UpdateChannelInfo(player.Id);
            UpdateChannelInfoAll();

            // TODO: players who are not in the channel will not be updated with its usercount
        }

        public void KickPlayer(OnlinePlayer player)
        {
            RemovePlayer(player);
            player.Connection.AddPendingPacket(new ChannelKickPacket(Name));
        }

        public void SendInfo(OnlinePlayer player)
        {
            player.Connection.AddPendingPacket(new ChannelPacket(this));
            if (ShouldAutoJoin)
            {
                player.Connection.AddPendingPacket(new ChannelAutoJoinPacket(this));
            }
        }
    }
}
