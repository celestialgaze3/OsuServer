using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class UsersStatsRequestPacketHandler : ClientPacketHandler
    {
        public UsersStatsRequestPacketHandler(byte[] data) 
            : base((int) ClientPacketType.UserStats, data) { }

        /// <summary>
        /// The client wants the stats of 
        /// </summary>
        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            OnlinePlayer? player = bancho.GetPlayer(osuToken);

            if (player == null) return Task.CompletedTask;
            List<int> requestedUserIds = reader.ReadIntListShortLength();

            // Reply by sending the user stats for all users requested
            foreach (int userId in requestedUserIds)
            {
                OnlinePlayer? requestedPlayer = bancho.GetPlayer(userId);
                if (requestedPlayer != null)
                {
                    player.Connection.AddPendingPacket(new UserStatsPacket(requestedPlayer));
                }
            }

            Console.WriteLine("Received a user stats request for " + requestedUserIds.Count + " users by " + player.Username);
            return Task.CompletedTask;
        }
    }
}
