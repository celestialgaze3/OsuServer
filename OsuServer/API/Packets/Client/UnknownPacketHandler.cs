using OsuServer.External.Database;
using OsuServer.State;

namespace OsuServer.API.Packets.Client
{
    public class UnknownPacketHandler : ClientPacketHandler
    {
        public UnknownPacketHandler(int id, byte[] data) 
            : base(id, data) { }

        protected override Task Handle(OsuServerDb database, Bancho bancho, string osuToken, BinaryReader reader)
        {
            Console.WriteLine("Received unknown packet with " + Id);
            Console.WriteLine("Data received: " + BitConverter.ToString(Data));
            Console.WriteLine("osu! token: " + osuToken);
            return Task.CompletedTask;
        }
    }
}
