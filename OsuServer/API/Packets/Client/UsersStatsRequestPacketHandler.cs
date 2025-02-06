using OsuServer.API.Packets.Server;
using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class UsersStatsRequestPacketHandler : ClientPacketHandler
    {
        public UsersStatsRequestPacketHandler(byte[] data, string osuToken, Bancho bancho) : base((int) ClientPacketType.UserStatsRequest, data, osuToken, bancho) { }

        /// <summary>
        /// The client wants the stats of 
        /// </summary>
        protected override Task Handle(OsuServerDb database, BinaryReader reader)
        {
            OnlinePlayer? player = Bancho.GetPlayer(Token);

            if (player == null) return Task.CompletedTask;
            List<int> requestedUserIds = reader.ReadIntListShortLength();

            // Reply by sending the user stats for all users requested
            foreach (int userId in requestedUserIds)
            {
                OnlinePlayer? requestedPlayer = Bancho.GetPlayer(userId);
                if (requestedPlayer != null)
                {
                    player.Connection.AddPendingPacket(new UserStatsPacket(requestedPlayer, player.Connection.Token, Bancho));
                }
            }

            Console.WriteLine("Received a user stats request for " + requestedUserIds.Count + " users by " + player.Username);
            return Task.CompletedTask;
        }
    }
}
