using OsuServer.API.Packets;

namespace OsuServer.State
{
    public class Connection
    {
        public string Token { get; private set; }
        public Bancho Bancho { get; private set; }

        private List<Packet> PendingPackets = [];

        public Connection(string token, Bancho bancho) 
        { 
            Token = token; 
            Bancho = bancho;
        }

        public void AddPendingPacket(Packet packet) {
            lock (PendingPackets)
            {
                PendingPackets.Add(packet);
            }
        }

        /// <summary>
        /// Packages all pending packets into a byte array
        /// </summary>
        /// <returns>A byte array to be put in a response body</returns>
        public byte[] FlushPendingPackets()
        {
            using var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                lock (PendingPackets)
                {
                    foreach (var packet in PendingPackets)
                    {
                        byte[] bytes = packet.GetBytes();
                        Console.WriteLine("Packet: " + BitConverter.ToString(bytes));
                        binaryWriter.Write(bytes);
                    }
                    PendingPackets.Clear();
                }
            }

            return memoryStream.ToArray();

        }
    }
}
