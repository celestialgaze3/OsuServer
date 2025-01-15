using OsuServer.State;
using System.Text;

namespace OsuServer.API.Packets.Client
{
    public class UnknownPacketHandler : ClientPacketHandler
    {
        public UnknownPacketHandler(int id, byte[] data, string osuToken, Bancho bancho) : base(id, data, osuToken, bancho) { }

        protected override void Handle(ref BinaryReader reader)
        {
            Console.WriteLine("Received unknown packet with " + Id);
            Console.WriteLine("Data received: " + BitConverter.ToString(Data));
            Console.WriteLine("osu! token: " + Token);
        }
    }
}
