using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class PresenceFilterPacketHandler : ClientPacketHandler
    {
        public PresenceFilterPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.PresenceFilter, data, osuToken, bancho) { }

        /// <summary>
        /// The client is sending its presence filter (telling us the user presences we should be sending to them)
        /// </summary>
        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;

            int value = reader.ReadInt32();
            if (value < 0 || value > 2)
            {
                Console.WriteLine("Client sent presence filter out of range: " + value);
                return Task.CompletedTask;
            }

            PresenceFilter filter = (PresenceFilter) value;
            player.Presence.Filter = filter;
            Console.WriteLine($"Updated {player.Username}'s presence filter to {value.ToString()}");
            return Task.CompletedTask;
        }
    }
}
