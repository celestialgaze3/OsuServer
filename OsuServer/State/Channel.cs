using OsuServer.API.Packets.Server;
using OsuServer.Objects;

namespace OsuServer.State
{
    public class Channel
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public List<int> Members { get; private set; } = new List<int>();
        private Bancho _bancho;

        public bool ShouldAutoJoin { get; private set; } = false;

        private bool Joinable = true; // TODO: implement permissions/private channels

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
        public bool AddPlayer(Player player)
        {
            if (!CanJoin(player))
            {
                return false;
            }

            Members.Add(player.Id);
            OnJoin(player);
            return true;
        }

        public bool CanJoin(Player player)
        {
            return Joinable;
        }

        public Channel AutoJoin()
        {
            ShouldAutoJoin = true;
            return this;
        }

        private void OnJoin(Player player)
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
            Player? player = _bancho.GetPlayer(playerId);
            if (player == null) return;

            player.Connection.AddPendingPacket(new ChannelPacket(this, player.Connection.Token, _bancho));
        }

        public bool HasMember(Player player)
        {
            return Members.Contains(player.Id);
        }

        public void SendMessage(OsuMessage message)
        {
            foreach (int memberId in Members)
            {
                Player? member = _bancho.GetPlayer(memberId);
                if (member == null) continue;

                if (member.Id != message.SenderId)
                {
                    member.SendMessage(message);
                }
            }
        }

        public void RemovePlayer(Player player)
        {
            if (!Members.Contains(player.Id)) return;
            Members.Remove(player.Id);

            // Now that the player has been removed, we need to tell both the player that left and the players in the channel about the updated usercount
            UpdateChannelInfo(player.Id);
            UpdateChannelInfoAll();

            // TODO: players who are not in the channel will not be updated with its usercount
        }

        public void KickPlayer(Player player)
        {
            RemovePlayer(player);
            player.Connection.AddPendingPacket(new ChannelKickPacket(Name, player.Connection.Token, _bancho));
        }

        public void SendInfo(Player player)
        {
            player.Connection.AddPendingPacket(new ChannelPacket(this, player.Connection.Token, _bancho));
            if (ShouldAutoJoin)
            {
                player.Connection.AddPendingPacket(new ChannelAutoJoinPacket(this, player.Connection.Token, _bancho));
            }
        }
    }
}
