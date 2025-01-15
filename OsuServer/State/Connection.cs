using OsuServer.API.Packets;
using OsuServer.API.Packets.Server;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OsuServer.State
{
    public class Connection
    {
        public string Token { get; private set; }
        public Bancho Bancho { get; private set; }

        private List<Packet> PendingPackets = new List<Packet>();

        public Connection(string token, Bancho bancho) 
        { 
            Token = token; 
            Bancho = bancho;
        }

        public void AddPendingPacket(Packet packet) { PendingPackets.Add(packet); }

        /// <summary>
        /// Packages all pending packets into a byte array
        /// </summary>
        /// <returns>A byte array to be put in a response body</returns>
        public byte[] FlushPendingPackets()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    foreach (var packet in PendingPackets)
                    {
                        Console.WriteLine("Packet: " + BitConverter.ToString(packet.GetBytes()));
                        binaryWriter.Write(packet.GetBytes());
                    }
                    PendingPackets.Clear();
                }

                return memoryStream.ToArray();
            }

        }
    }
}
